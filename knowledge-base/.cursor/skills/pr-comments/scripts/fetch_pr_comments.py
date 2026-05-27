#!/usr/bin/env python3
"""
Fetch PR comments from GitHub with thread grouping and resolved status.
Outputs structured JSON for agent analysis.

Requires: GitHub CLI (gh) installed and authenticated.

Usage:
    python fetch_pr_comments.py              # auto-detect PR from current branch
    python fetch_pr_comments.py --pr 123     # specific PR number
"""

import json
import subprocess
import sys
import argparse


def run_gh(*args):
    """Run a gh CLI command and return stdout. Exit with JSON error on failure."""
    result = subprocess.run(["gh", *args], capture_output=True, text=True)
    if result.returncode != 0:
        error_msg = result.stderr.strip() or "Unknown error"
        print(json.dumps({"error": f"gh command failed: {error_msg}"}), file=sys.stderr)
        sys.exit(1)
    return result.stdout.strip()


def get_repo_nwo():
    """Get 'owner/repo' for current directory."""
    raw = run_gh("repo", "view", "--json", "nameWithOwner", "-q", ".nameWithOwner")
    return raw


def get_pr_info(pr_number=None):
    """Get PR metadata. Auto-detects from current branch if no number given."""
    fields = "number,title,url,author,headRefName,baseRefName"
    if pr_number:
        raw = run_gh("pr", "view", str(pr_number), "--json", fields)
    else:
        raw = run_gh("pr", "view", "--json", fields)
    return json.loads(raw)


def fetch_review_threads(owner, repo, pr_number):
    """Fetch review threads via GraphQL — includes resolved status and threaded comments."""
    all_threads = []
    cursor = None

    while True:
        after_clause = f', after: "{cursor}"' if cursor else ""

        query = f"""{{
  repository(owner: "{owner}", name: "{repo}") {{
    pullRequest(number: {pr_number}) {{
      reviewThreads(first: 100{after_clause}) {{
        nodes {{
          id
          isResolved
          isOutdated
          line
          startLine
          path
          comments(first: 100) {{
            nodes {{
              databaseId
              body
              author {{ login }}
              createdAt
              url
            }}
          }}
        }}
        pageInfo {{
          hasNextPage
          endCursor
        }}
      }}
    }}
  }}
}}"""

        raw = run_gh("api", "graphql", "-f", f"query={query}")
        data = json.loads(raw)

        if "errors" in data:
            print(json.dumps({"error": f"GraphQL errors: {data['errors']}"}), file=sys.stderr)
            sys.exit(1)

        pr_data = data.get("data", {}).get("repository", {}).get("pullRequest")
        if not pr_data:
            print(json.dumps({"error": "Could not fetch PR data via GraphQL"}), file=sys.stderr)
            sys.exit(1)

        threads_data = pr_data["reviewThreads"]
        all_threads.extend(threads_data["nodes"])

        if not threads_data["pageInfo"]["hasNextPage"]:
            break
        cursor = threads_data["pageInfo"]["endCursor"]

    return all_threads


def fetch_issue_comments(owner, repo, pr_number):
    """Fetch general PR comments (not tied to specific code lines)."""
    raw = run_gh("api", "--paginate", f"/repos/{owner}/{repo}/issues/{pr_number}/comments")
    if not raw:
        return []
    return json.loads(raw)


def fetch_reviews(owner, repo, pr_number):
    """Fetch PR review summaries (APPROVED, CHANGES_REQUESTED, etc.)."""
    raw = run_gh("api", "--paginate", f"/repos/{owner}/{repo}/pulls/{pr_number}/reviews")
    if not raw:
        return []
    return json.loads(raw)


def build_output(pr_info, review_threads, issue_comments, reviews):
    """Build structured output for agent analysis."""
    processed_threads = []
    for thread in review_threads:
        comments = []
        for c in thread.get("comments", {}).get("nodes", []):
            author = c.get("author")
            comments.append({
                "id": c.get("databaseId"),
                "author": author["login"] if author else "[deleted]",
                "body": c.get("body", ""),
                "created_at": c.get("createdAt", ""),
                "url": c.get("url", ""),
            })

        processed_threads.append({
            "id": thread["id"],
            "status": "resolved" if thread["isResolved"] else "unresolved",
            "is_outdated": thread["isOutdated"],
            "file": thread.get("path"),
            "line": thread.get("line"),
            "start_line": thread.get("startLine"),
            "comment_count": len(comments),
            "comments": comments,
        })

    processed_issue_comments = []
    for c in issue_comments:
        user = c.get("user")
        processed_issue_comments.append({
            "id": c.get("id"),
            "author": user["login"] if user else "[deleted]",
            "body": c.get("body", ""),
            "created_at": c.get("created_at", ""),
            "url": c.get("html_url", ""),
        })

    processed_reviews = []
    for r in reviews:
        body = r.get("body", "")
        if not body:
            continue
        user = r.get("user")
        processed_reviews.append({
            "author": user["login"] if user else "[deleted]",
            "state": r.get("state", ""),
            "body": body,
            "created_at": r.get("submitted_at", ""),
        })

    return {
        "pr": {
            "number": pr_info["number"],
            "title": pr_info["title"],
            "url": pr_info["url"],
            "author": pr_info["author"]["login"] if pr_info.get("author") else "[unknown]",
            "branch": pr_info.get("headRefName", ""),
            "base": pr_info.get("baseRefName", ""),
        },
        "summary": {
            "total_review_threads": len(processed_threads),
            "unresolved_threads": sum(1 for t in processed_threads if t["status"] == "unresolved"),
            "resolved_threads": sum(1 for t in processed_threads if t["status"] == "resolved"),
            "outdated_threads": sum(1 for t in processed_threads if t["is_outdated"]),
            "general_comments": len(processed_issue_comments),
            "reviews_with_body": len(processed_reviews),
        },
        "review_threads": processed_threads,
        "general_comments": processed_issue_comments,
        "reviews": processed_reviews,
    }


def main():
    parser = argparse.ArgumentParser(description="Fetch PR comments for analysis")
    parser.add_argument("--pr", type=int, help="PR number (default: auto-detect from current branch)")
    args = parser.parse_args()

    nwo = get_repo_nwo()
    owner, repo = nwo.split("/", 1)

    pr_info = get_pr_info(args.pr)
    pr_number = pr_info["number"]

    review_threads = fetch_review_threads(owner, repo, pr_number)
    issue_comments = fetch_issue_comments(owner, repo, pr_number)
    reviews = fetch_reviews(owner, repo, pr_number)

    output = build_output(pr_info, review_threads, issue_comments, reviews)
    print(json.dumps(output, indent=2))


if __name__ == "__main__":
    main()
