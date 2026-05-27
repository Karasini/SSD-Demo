---
name: create-pr
description: Creates a Pull Request with a short English title and English description from the pr-description template. Derives the title from the branch diff and branch name when possible. Uses GitHub CLI and a project script. Use when the user asks to create a PR or open a pull request.
---

# Create PR

Create a Pull Request with a **short English title** and an **English description** generated from the branch diff and [pr-description](../commands/pr-description.md).

## Prerequisites

- GitHub CLI (`gh`) installed and authenticated
- Current directory in the git repository (branch pushed to remote when using a remote base)

## Step 1: Current branch

Get the current branch:

```bash
git branch --show-current
```

Use the branch name and slug only as hints for the title (e.g. `feature/add-admin-endpoints` → “Add admin endpoints”). Do not require a specific task-ID prefix or tracker format.

## Step 2: PR title (English)

**Format:** Short English phrase, **max 5–6 words**, summarizing the main change (same rules as [pr-description](../commands/pr-description.md)).

**How to choose the title:**

1. Prefer a title inferred from the **@Branch** diff (main behaviour change).
2. If unclear, derive from the branch slug (human-readable words, not raw kebab-case).
3. If still unclear, **ask the user** for a short title before creating the PR.

**Language:** English only. Active voice when possible (e.g. “Fix email loading”, “Add admin endpoints”).

**Do not** require a ticket prefix (e.g. `PROJ-123`) unless the user explicitly asks for it.

## Step 3: Generate PR description (body)

Generate the description in English using the **fixed template** and rules from [.cursor/commands/pr-description.md](../commands/pr-description.md):

1. Use **@Branch** (diff of current branch against `main`) as input.
2. Consider `.cursor` (rules, commands, skills) for context and “Deviations from norms”.
3. Fill every section of the template (Significant behaviour changes, Deviations from norms, Libraries, Visual representation, PR author comment).
4. Output **raw Markdown only** for the full template output (no code fence around the whole output).
5. Leave “PR author comment” as the placeholder `<!-- -->` block.

**GitHub fields:**

- **`--title`:** Use the generated **PR title** (section “PR title” — max 5–6 words). Do not put the title only inside the body.
- **`--body` / `--body-file`:** Use the **PR description** part only — from `### 1. Significant application behaviour changes` through section 5. Do not repeat the short title at the top of the body.

Write the body to a temporary file (e.g. `pr_body.md`) for the script.

## Step 4: Create the PR and capture URL

Run the project script (preferred) or `gh pr create` directly:

```bash
python .cursor/skills/create-pr/scripts/create_pr.py --title "Add admin endpoints" --body-file pr_body.md
```

Or with inline body (shorter descriptions):

```bash
python .cursor/skills/create-pr/scripts/create_pr.py --title "Add admin endpoints" --body "$(cat pr_body.md)"
```

The script runs `gh pr create --base main --title "..." --body-file ...` and prints the PR URL. Capture that URL.

If the default base branch is not `main`, pass the correct base to `gh` or adjust the script for this repo.

## Step 5: Return the PR link to the user

**Always** show the user the link to the created PR, for example:

- “PR created: https://github.com/owner/repo/pull/123”
- Or as a markdown link: `[PR #123](https://github.com/owner/repo/pull/123)`

Do not finish the workflow without returning this link.

## Summary checklist

- [ ] Get current branch (optional hint for title only).
- [ ] PR title = short English phrase (max 5–6 words) from diff, branch slug, or user input.
- [ ] Generate PR body from [pr-description.md](../commands/pr-description.md) using @Branch and template (description sections only in body).
- [ ] Run `create_pr.py --title "..." --body-file pr_body.md` (or `--body`); capture printed URL.
- [ ] **Always** return the PR link to the user.
