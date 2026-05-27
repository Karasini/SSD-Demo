---
name: pr-comments
description: Fetch and analyze GitHub PR comments for the active pull request. Groups threaded comments, checks resolved status, and generates a recommendation table with actionable insights. Use when the user asks to review PR comments, analyze PR feedback, check what needs to be fixed in a PR, or mentions PR review comments.
---

# PR Comments Analysis

Analyze GitHub PR comments and generate actionable recommendations.

## Prerequisites

- GitHub CLI (`gh`) installed and authenticated (`gh auth login`)
- Current directory inside a git repository with a GitHub remote

## Workflow

### Step 1: Fetch PR Comments

Run the fetch script:

```bash
python .cursor/skills/pr-comments/scripts/fetch_pr_comments.py
```

To target a specific PR:

```bash
python .cursor/skills/pr-comments/scripts/fetch_pr_comments.py --pr 123
```

The script outputs JSON with:
- `review_threads` — code-specific comment threads with `resolved`/`unresolved` status and `is_outdated` flag
- `general_comments` — PR-level discussion comments (not tied to code)
- `reviews` — review summaries with state (APPROVED, CHANGES_REQUESTED, etc.)
- `summary` — aggregated counts

### Step 2: Analyze Comments

For each **review thread**:

1. **Read the full conversation** — comments are chronologically ordered. First comment = original review remark; subsequent = replies.
2. **Check status flags**:
   - `resolved` — reviewer explicitly marked as resolved; likely no action unless fix is missing
   - `unresolved` — needs attention
   - `is_outdated` — the code changed since this comment; verify relevance
3. **Assess conversation outcome**:
   - Author agreed to fix → check if fix was actually applied
   - Author pushed back with reasoning → may be fine to skip
   - Ongoing disagreement → flag for discussion
   - Question that got answered → no action needed
4. **Read the referenced file** in the codebase (use the `file` and `line` fields) to verify whether the concern has been addressed in the current code

For **general comments** and **review summaries**:
- Extract actionable items
- Note overall review stance

### Step 3: Generate Recommendation Table

Present findings using this template:

```
## PR Comments Analysis: PR #[number] — [title]

**Summary:** [X] unresolved, [Y] resolved, [Z] outdated threads | Reviews: [states]

### Recommendations

| # | File | Topic | Status | Action | Reason |
|---|------|-------|--------|--------|--------|
| 1 | `path/file.cs:42` | Brief description | Unresolved | **Fix** | Why this should be fixed |
| 2 | `path/other.cs:10` | Brief description | Resolved | **Skip** | Already addressed |
| 3 | `path/bar.cs:5` | Brief description | Outdated | **Verify** | Code changed since comment |
| 4 | General | Topic | — | **Discuss** | Needs team input |

### Legend
- **Fix** — implement the change
- **Skip** — no action needed
- **Verify** — check if concern is still relevant after recent changes
- **Discuss** — needs discussion, disagreement, or team input
```

### Recommendation Criteria

| Action | When to assign |
|--------|---------------|
| **Fix** | Unresolved + valid concern + author hasn't pushed back, or author agreed but hasn't implemented |
| **Skip** | Resolved, or convincing counter-argument accepted by reviewer, or pure style nitpick |
| **Verify** | Outdated thread, or author claims fix but thread not resolved — read the file to check |
| **Discuss** | Ongoing disagreement, architectural concern, or needs broader team input |

### After the Table

Add a brief section covering:
- Recurring themes across comments (e.g., "3 comments about missing null checks")
- Review-level feedback not tied to specific threads
- Suggested priority order for fixes (architectural concerns first, then smaller items)
