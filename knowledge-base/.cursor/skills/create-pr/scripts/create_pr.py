#!/usr/bin/env python3
"""
Create a Pull Request via GitHub CLI with given title and body.
Prints the PR URL on success. Used by the create-pr skill.

Requires: GitHub CLI (gh) installed and authenticated.

Usage:
    python create_pr.py --title "Add admin endpoints" --body "## PR description\n..."
    python create_pr.py --title "Add admin endpoints" --body-file pr_body.md
"""

import argparse
import subprocess
import sys


def run_gh_create(title: str, body: str | None, body_file: str | None) -> str:
    """Run gh pr create; return PR URL from stdout."""
    cmd = ["gh", "pr", "create", "--base", "main", "--title", title]
    if body_file:
        cmd.extend(["--body-file", body_file])
    elif body is not None:
        cmd.extend(["--body", body])
    else:
        print("Error: provide --body or --body-file", file=sys.stderr)
        sys.exit(1)

    result = subprocess.run(cmd, capture_output=True, text=True)
    if result.returncode != 0:
        print(result.stderr, file=sys.stderr)
        sys.exit(1)
    return result.stdout.strip()


def main():
    parser = argparse.ArgumentParser(description="Create PR with title and body; print PR URL")
    parser.add_argument("--title", required=True, help="PR title in English (e.g. Add admin endpoints)")
    group = parser.add_mutually_exclusive_group(required=True)
    group.add_argument("--body", help="PR body (Markdown)")
    group.add_argument("--body-file", help="Path to file containing PR body (Markdown)")
    args = parser.parse_args()

    body = args.body if args.body else None
    body_file = args.body_file if args.body_file else None
    url = run_gh_create(args.title, body, body_file)
    print(url)


if __name__ == "__main__":
    main()
