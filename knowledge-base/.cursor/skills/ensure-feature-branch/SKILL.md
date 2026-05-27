---
name: ensure-feature-branch
description: Before starting work, ensures we are not on main; if on main, creates and checks out a branch as feature|fix|refactor/<slug>. Infers type and slug from the user message or asks when unclear. Use when starting work, when the user asks to create a branch, switch branch, or ensure branch, or when they are on main and need to work.
---

# Ensure Feature Branch

Before starting work, verify we are on a feature branch. Never work on `main`. If the current branch is `main` (or `master`), create and switch to a new branch using the naming convention below.

## Prerequisites

- Git repository; current directory in repo root or any subdirectory

## Step 1: Check current branch

Run:

```bash
git branch --show-current
```

If the result is `main` or `master`, proceed to Step 2. Otherwise **stop** — no branch change needed. Tell the user they are already on a non-main branch.

## Step 2: Branch type and slug

### Branch type

Choose one based on the work:

| Type        | When to use                          |
|-------------|--------------------------------------|
| `feature`   | New functionality                  |
| `fix`       | Bug fix                            |
| `refactor`  | Refactoring, code cleanup          |

Infer from the user message (e.g. “bug fix” → `fix`, “new feature” → `feature`, “refactor” → `refactor`). If unclear, ask: “Is this a feature, fix, or refactor?”

### Slug

Short, lowercase, hyphen-separated name describing the work. Examples:

- “Admin endpoints” → `admin-endpoints`
- “Email loading fix” → `email-loading-fix`
- “Delete demo” → `delete-demo`

Use the user’s description or a short summary of the planned work. No spaces or special characters.

If the slug is unclear, ask the user for a short name (2–4 words) and convert it to a slug.

## Step 3: Branch naming convention

Format:

```
<type>/<slug>
```

Examples:

- `feature/admin-endpoints`
- `fix/email-loading-fix`
- `refactor/delete-demo`

Rules:

- `type`: `feature` | `fix` | `refactor`
- `slug`: lowercase, hyphens, no spaces

Do **not** require a ticket or task ID in the branch name unless the user explicitly asks for one (e.g. `feature/PROJ-123-admin-endpoints`).

## Step 4: Create and checkout branch

Run:

```bash
git checkout -b <type>/<slug>
```

Example:

```bash
git checkout -b feature/admin-endpoints
```

Confirm success (e.g. “Switched to a new branch 'feature/admin-endpoints'”) and tell the user they are on the new branch and can start work.

## Summary checklist

- [ ] Run `git branch --show-current`; if not `main` or `master`, stop.
- [ ] Choose type: `feature` | `fix` | `refactor`.
- [ ] Build slug from user description (lowercase, hyphens); ask if unclear.
- [ ] Run `git checkout -b <type>/<slug>`.
- [ ] Confirm branch switch to the user.
