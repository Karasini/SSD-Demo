---
name: create-feature
description: Guides gathering and challenging requirements, then creating structured overview, usecases, UX/UI specification, and technical-specification stubs for a feature folder. Use when the user wants to describe a new or existing feature with the standard four-document structure in the knowledge-base repository.
---

# Feature Specification Guidance

## Purpose

This skill helps the agent:

- Clarify and challenge the user's idea for a new or existing feature.
- Guide the user through consistent documentation in the **knowledge-base** (or agreed docs repo):
  - `overview.md` – business problem and goals (simple English B2).
  - `usecases.md` – use cases and supported behaviors (simple English B2).
  - `specification-ux-ui.md` – UX/UI proposal for design work; refine later when design exists.
  - `technical-specification.md` – placeholder only at this stage (empty file or minimal `TODO`).
- Ensure all documents follow the corresponding rules in `.cursor/rules/`.

## Scope and location

- Work in the repository that holds feature documentation (typically the **knowledge-base** project).
- Use the folder layout the team already uses, or agree one with the user. A common pattern:

  ```
  <scope-or-product>/Features/<N. Feature name>/
    overview.md
    usecases.md
    specification-ux-ui.md
    technical-specification.md
  ```

- Examples: `product-a/Features/2. Mobile calendar/`, `features/billing-export/`.
- Do not invent a different layout without the user's approval.

## Assets

Starter templates in `assets/`:

- `assets/overview.template.md`
- `assets/usecases.template.md`
- `assets/specification-ux-ui.template.md`
- `assets/technical-specification.todo.md`

When creating new files: start from templates, adapt to gathered requirements, and treat `.cursor/rules/*.mdc` as the source of truth for quality and structure.

## General workflow

When the user wants to describe a feature (new or existing):

1. **Clarify feature scope and location.**
2. **Challenge and refine the problem and goals.**
3. **Derive and validate key use cases.**
4. **Prepare a UX/UI proposal for design.**
5. **Create or update the four files in priority order:**
   1. `overview.md` (required).
   2. `usecases.md` (required).
   3. `specification-ux-ui.md` (required; filled proposal, not empty).
   4. `technical-specification.md` (required; empty or minimal `TODO` only).

Do **not** jump straight to writing files. Ask targeted questions and challenge ambiguities first.

## Step 1 – Clarify scope and folder

Before writing anything:

1. Ask the user to confirm:
   - **Scope / product area** (folder prefix or domain name).
   - Whether this is a **new** feature or an **update** to an existing one.
2. If the folder does not exist, propose a path and naming (with numbering if the team uses it).
3. If the feature already exists, detect and confirm the exact folder path.

Example questions:

- "Which product or area does this feature belong to?"
- "Is this a new feature or an update? If an update, what is the folder path or name?"
- "What folder name should we use (e.g. `2. Mobile calendar`)?"

Only after the path is clear, proceed to requirements clarification.

## Step 2 – Clarify and challenge problem & goals (for `overview.md`)

Rules: `.cursor/rules/overview-documents.mdc` and `.cursor/rules/english-simple-b2-documents.mdc`.

Key constraints:

- **Simple English (B2).**
- Audience: non-technical readers.
- Scope: **only** problem and goals — no implementation details.

Collect enough input for:

- Who has the problem.
- What hurts today.
- Repeated situations where the pain appears.
- What should become clearly better after delivery.
- What is explicitly out of scope.

Targeted questions:

1. **Problem**
   - "Who exactly has this problem (role, company type, context)?"
   - "What is most frustrating or time-consuming today?"
   - "What 2–4 situations best show this pain?"
2. **Goals**
   - "What should change for the user after we ship this?"
   - "How will we know the goal was met? What behavior or outcome changes?"
   - "Are there goals this feature **deliberately does not** address?"

Challenge vague statements:

- "It should be more convenient" → "In which situations? What will the user do differently?"
- "Better visibility" → "What information, for whom, and when?"
- User describes only UI → step back to business need and user outcome.
- User mixes problem and solution → "What problem are we solving, regardless of a specific screen?"

Checkpoint before `overview.md`:

- Summarize the problem in 2–4 bullets.
- Summarize goals in 2–5 bullets.
- List assumptions or open decisions.
- If still vague, ask more questions instead of writing the file.

### Generating `overview.md`

Structure per `overview-documents.mdc`:

- `Problem` – short intro + 2–4 bullets.
- `Goals` – short paragraph + specific goal bullets.

Ensure:

- Simple language; no technical jargon or marketing fluff.
- No implementation, UI, endpoints, or architecture.

If `overview.md` exists: read it, propose incremental improvements, briefly explain what will change.

## Step 3 – Derive and challenge use cases (for `usecases.md`)

Rules: `.cursor/rules/usecases-documents.mdc`.

Key constraints:

- **Simple English (B2).**
- Focus on concrete use cases, not implementation.

Validate with the user:

- main actor(s),
- most important recurring workflows,
- critical day-one success scenarios,
- important edge cases or exceptions.

Questions:

- "What are the main ways people use this in a typical workday?"
- "Who uses it — what is their role and responsibility?"
- "What must work on day one for the feature to be useful?"
- "Are there critical edge cases we must include?"

For each candidate use case clarify:

- **Actor** – who acts.
- **Trigger** – what starts the flow.
- **What needs to be supported** – testable capabilities.

Challenge vague descriptions:

- "User handles orders" → "What exact actions? What can they filter, see, and do to finish the flow?"
- "It should work like a calendar" → "What 3–5 tasks should be faster or more reliable?"
- "Everyone will use it" → ask for a primary role and optional secondary roles.

Checkpoint before `usecases.md`:

- list candidate use cases,
- mark must-have for v1 vs optional/future,
- **size check**: if there are more than **8–10** use cases, warn that the feature may be too large; suggest splits (phases, v1 vs later) but **do not split without user approval**,
- confirm wording when ambiguous.

### Generating `usecases.md`

Pattern per `usecases-documents.mdc`:

- `## Use Case N: [Short, concrete title]`
- `**Actor:** ...`
- `**Trigger:** ...`
- `**What needs to be supported:**` – bullets starting with verbs, each testable.

Avoid: implementation details, pixel-level UI, long narratives.

If `usecases.md` exists: read, map to current requirements, add missing cases, simplify unclear ones.

## Step 4 – Create UX/UI proposal (`specification-ux-ui.md`)

Rules: `.cursor/rules/specification-ux-ui-documents.mdc`.

Important:

- **Always** ensure `specification-ux-ui.md` exists.
- Useful as **input for UX/UI design**, even before final Figma (or equivalent).
- Mark assumptions and open questions when design is not final.

1. Ask the user:
   - "How detailed should the UX/UI proposal be now — light concept or concrete enough for design?"
   - "Do you already have Figma (or other design) links or source materials?"
2. Create the file if missing; read it first if it exists.
3. **Always** fill a working proposal — never leave it empty or only a TODO list.
4. For a lighter proposal: high-level layout, core views, filters, details, missing states; mark areas needing design validation.
5. For a concrete proposal: confirm design links; follow section order from the rule:
   1. `Source materials`
   2. `Base layout`
   3. `Views / screen variants`
   4. `Components, cards, and visual variants`
   5. `Filters, search, and view controls`
   6. `Additional panels / modals / drawers / details`
   7. `UI states not visible in design` (+ optional `Open questions for UX/UI`)

When generating or updating:

- Concise, decision-oriented; prefer lists/subsections over huge tables (per rule).
- Treat as a **proposal**, not frozen final design.
- Separate: confirmed inputs, proposed decisions, open questions.
- Cover especially: loading, empty, error, filters/search, detail panel behavior, states not visible in design files.

Do not include: implementation details; copying the design file without behavioral decisions.

## Step 5 – Technical specification placeholder (`technical-specification.md`)

Rules: `.cursor/rules/technical-specification-documents.mdc`.

At this stage:

- **Do not** write a full technical solution.
- Always ensure `technical-specification.md` exists.
- Default: empty file, or content from `assets/technical-specification.todo.md` (`TODO` only).

Do **not** add endpoints, flows, or option analysis here unless the user explicitly asks later (use the `create-technical-specification` skill).

## Interaction style and guardrails

- Ask clarifying questions before writing documents.
- Challenge vague or conflicting statements.
- Keep the user in the loop: which file you are writing, key assumptions before committing.
- Prioritize **overview** and **usecases** — do not skip them.
- Prefer one focused batch of discovery questions, then a summary, then follow-ups — avoid writing too early.

If critical information is missing (no clear user, goal, or example situations): stop and ask instead of guessing.
