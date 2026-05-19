---
name: create-dev-tasks
description: "Breaks a user story and/or feature specs into multiple FE/BE developer tasks with clear acceptance criteria oriented on business value per task. Use when the user wants to create task markdown files under `tasks/` either per existing user story folder or at the feature folder level, based on `specification-ux-ui.md` and `technical-specification.md`"
---

# Create Dev Tasks From Spec

## Purpose

This skill turns a **user story** and/or feature specs into a **small set of concrete, deliverable tasks for developers** as Markdown files under the correct `tasks/` folder (per user story folder or at the feature level).

The focus is on:

- extracting **implementable chunks of work** from detailed specs,
- keeping each task **clear, self-contained, and testable**,
- avoiding over-explaining what the spec already says.

**Language:** simple English (B2) — see `.cursor/rules/english-simple-b2-documents.mdc`.

---

## When to use

Use this skill when:

- the user has:
  - `overview.md`, user story files, and especially `technical-specification.md` for a feature, **and**
  - wants **concrete developer tasks** for that user story or feature,
- the scope is **larger than one small task** and needs to be split into several parts,
- the user mentions:
  - "break this into dev tasks",
  - "tasks for this user story / spec",
  - "what should the dev do for this".

Do **not** use this skill if:

- the user wants a **single task description** from a short prompt — use `create-task` instead,
- the change is very small (one endpoint, one migration) — generate **one** task with `create-task` instead of a full list.

---

## Overall workflow

Always follow these phases:

1. **Input & scope understanding**
2. **Spec analysis & grouping**
3. **Task slicing (how many tasks + BE/FE split?)**
4. **Task description format (as files)**
5. **Output: create task files in `tasks/`**

---

## 1. Input & scope understanding

1. **Treat all provided context as part of the spec**:
   - pasted user story text,
   - links/paths to `overview.md`, `technical-specification.md`, user story files,
   - additional chat commentary.

2. **Resolve the target feature folder** with the user:
   - source of `overview.md`, `specification-ux-ui.md`, `technical-specification.md`, and user stories (path depends on team layout).

3. **Decide where tasks should be created (two scenarios)**:
   - **Scenario A (user story folders exist):** if the feature has `user-stories/` (or similar) with story subfolders, create `tasks/` **inside each story folder**.
     - example: `<feature-folder>/user-stories/<NN>-<story-name>/tasks/`
   - **Scenario B (no user stories):** create `tasks/` **inside the feature folder**.
     - example: `<feature-folder>/tasks/`
   - If unclear, ask a short confirmation before creating files.

4. **Do not rephrase the whole spec** — understand:
   - what is explicitly **in scope** (flows, endpoints, pipeline steps, FE views/interactions),
   - what is explicitly **out of scope**.

5. **If the user did not specify task count or granularity**:
   - confirm scope boundary first (what “done” means end-to-end),
   - decide split size in step 3 using `technical-specification.md` + `specification-ux-ui.md`.

If something is ambiguous, you may add `[to be confirmed]` in the task, but **do not stop** — propose a reasonable default.

---

## 2. Spec analysis & grouping

Before writing tasks:

1. **Read `technical-specification.md`** (if provided):
   - endpoints (HTTP, messaging, schedulers),
   - contracts (request/response, events),
   - modules/handlers/pipelines affected,
   - migrations/infra requirements.
2. **Read `specification-ux-ui.md`**:
   - new or changed views/components,
   - interaction rules (filters, states, empty/error UX),
   - data the UI needs and behavior across states.
3. **Check project rules** in `.cursor/rules` (in knowledge-base and implementation repos) when relevant:
   - messaging/events,
   - AI integration,
   - observability,
   - database migrations.
4. **Identify natural work streams**, split into:
   - **BE stream(s)** (from `technical-specification.md`): contracts, endpoints, handlers, jobs, migrations.
   - **FE stream(s)** (from `specification-ux-ui.md`): **user-visible capabilities / flows** (not “one task per technical layer”), each with end-to-end UX (including loading/empty/error), API wiring, and data refresh needed for the same product value.

Example grouping (illustrative):

- "BE: calendar grid endpoints (GridCalendar contract)",
- "BE: filter metadata for grid",
- "FE: desktop grid — full grid mode (view + filters + list states + V1 contract wiring)",
- "FE: separate task only when a second user flow is independent (different perspective or action in another place)".

---

## 3. Task slicing principles

When deciding **what is one task**:

- **Deliverable size**:
  - 1 task ≈ what **one developer** can finish in a reasonable time (e.g. 0.5–2 days).
  - Avoid both “epic” tasks and micro-tasks like “add one field to a class”.
- **One main behavior per task**:
  - e.g. "BE: new endpoint + handler + validation for GridCalendar V1",
  - or "FE: new grid view + filter/empty/error logic per UX spec".
- **Minimal dependencies between tasks**:
  - split for parallel work when possible,
  - if order is strict, note it under **Dependencies** (in task description or requirements).
- **Do not repeat the technical spec**:
  - the task should say **which part of the spec** this slice implements, not copy the whole spec.

Heuristics:

- **New endpoint / new BE contract area** → usually its own task.
- **New response type / read model** → own task unless inseparable from the endpoint.
- **Migrations / scripts** → own task.
- **Refactor / cleanup** from the feature → own task, label as refactor or cleanup.
- **New view / screen / user flow** with real UX logic → usually **one** FE task (UI + API calls + cache refresh) if it delivers one business outcome.
- **Separate FE task for “types only / hook only / styles only”** → **avoid**, unless a short spike with a clear goal (mark as spike).

Aim for **3–8 sensible tasks** for a medium feature. If you get 1–2, use `create-task`. If you get more than 10, merge very small steps into larger logical units.

### Requirement: always separate BE and FE

Always generate separate tasks for:

- **BE** (from `technical-specification.md`): contracts, endpoints, handlers, pipeline steps, migrations.
- **FE** (from `specification-ux-ui.md`): **full user flow** (UI + states + API + cache refresh in scope), not layer-by-layer splits.

There may be multiple BE and FE tasks depending on spec size and number of independent product capabilities on the UI side.

### When the user does not specify task count

1. Confirm scope from the story/feature (what “done” means end-to-end).
2. From `technical-specification.md`, decide BE task count (independent endpoint/slice/migration areas).
3. From `specification-ux-ui.md`, decide FE task count by **independent user-facing capabilities** (separate flow, perspective, or action context), not by component or file count.
4. Then produce the final BE/FE task list.

**AC quality check (BE and FE):** before saving files, check **each** task — can it be marked “done” **only from Acceptance criteria** (without the technical section and without file names / implementation identifiers in AC)? If not — merge with a neighbor or rewrite AC.

---

## 4. Task description format (as files)

Each task is a **separate Markdown file** in `tasks/` (e.g. `tasks-backend-*.md`, `tasks-frontend-*.md`).

**File template:**

```markdown
## [BE] <concrete title>

[1–3 sentences: what we build and for which view/flow. Reference `specification-ux-ui.md` and/or `technical-specification.md` at a high level.]

---
## Acceptance criteria (business value)
1. [...]
2. [...]
3. [...]

---
## Task description
1. [...]
2. [...]
3. [...]

---
## Requirements (for developer)
1. [...]
2. [...]
3. [...]
```

Rules:

- **Language:** simple English (B2).
- **Tone:** factual, concrete, no marketing.
- **First heading in file:** `## [BE] …` for backend or `## [FE] …` for frontend (tags in **uppercase**). Do **not** use `[backend]` / `[frontend]` in new tasks — the heading should match the **Summary** field in YouTrack when syncing (e.g. `sync-feature-to-youtrack`).
- **Section order:** short intro (1–3 sentences), then **Acceptance criteria**, then **Task description**, then **Requirements**.
- **Acceptance criteria** → product/QA/business (“done” signals),
- **Task description** → what to implement in this slice,
- **Requirements** → developer checklist (what to build, behaviors, tests).
- **Do not copy large spec chunks** — refer at a high level (contracts, views, section names).

---

## 5. Output: create task files in `tasks/`

1. Create `tasks/` per section 1:
   - Scenario A: `<feature-folder>/user-stories/<NN>-<story-name>/tasks/`
   - Scenario B: `<feature-folder>/tasks/`
2. For each task, create one Markdown file:
   - `tasks-backend-<kebab-case>.md` for BE
   - `tasks-frontend-<kebab-case>.md` for FE
3. Each file follows the format in section 4.
4. Do not overwrite existing files:
   - if a file exists, use a new name (suffix) or ask the user.
5. Suggested generation order:
   - BE contracts/migrations first,
   - then dependent BE/FE tasks.
6. **List tasks before files:** propose **BE** and **FE** titles with one line of value each, run the **AC quality check** and section 3 heuristics, then generate `tasks-backend-*.md` / `tasks-frontend-*.md`.

---

## Examples (conceptual)

### Example: calendar feature (BE + FE)

Input (simplified):

- `technical-specification.md` describes:
  - new endpoint `/schedule-items/calendar/grid` + V1 contract,
  - metadata endpoints for the grid,
  - alias `/schedule-items/calendar/schedule` for schedule view,
  - versioning rules for API slices.

Expected tasks (high level):

- BE: implement `/schedule-items/calendar/grid` (GridCalendar),
- BE: grid metadata endpoints `/schedule-items/calendar/grid/categories` and `/schedule-items/calendar/grid/statuses`,
- FE: **one** task: desktop grid view + filters + empty/loading/error + grid/metadata API calls + mapping to view structures (AC verifiable end-to-end).

### Example task file patterns

Look at existing `tasks/` folders in the repo for **tone, acceptance criteria style, and slice size** (separate BE and FE files).

Typical names:

- `tasks-backend-<topic>.md`
- `tasks-frontend-<topic>.md`

Older files may use `## [backend]` / `## [frontend]` — **legacy**. New tasks use only `## [BE]` / `## [FE]`.

---

## Additional notes

- This skill does **not** replace `create-task` — it produces **several tasks** that can be refined with `create-task` if one task stays too broad.
- Avoid copying large spec sections — refer verbally (e.g. “per section 3.2 of technical-specification.md”).
- Stay consistent with:
  - `.cursor/rules` in the workspace,
  - ADRs in `docs/adr/` when present.
