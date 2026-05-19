---
name: create-user-stories
description: Create user story files for a feature in Responia-Knowledge. Use when the user wants to write one or more user stories for a feature under Responia/Features. Analyses existing feature docs (overview.md, usecases.md, specification-ux-ui.md, technical-specification.md) to propose the right number and granularity of stories, then generates properly formatted files in the feature user-stories/ folder.
argument-hint: 'Feature folder path or name and optional scope hint (e.g. "one story per use case" or "single story")'
user-invocable: true
disable-model-invocation: false
---

# User Story Creation (Responia-Knowledge)

## Purpose

This skill helps the agent:
- locate the correct feature folder and its `user-stories/` subfolder
- analyse existing feature documentation to determine the right scope and number of user stories
- propose a story split plan before generating any files
- create properly formatted `*-userstory.md` files following the repo instruction

## Repository Scope

Always work inside this structure in `Responia-Knowledge`:

- `Responia/Features/...`

User stories always live in a `user-stories/` subfolder directly inside the feature folder:

```markdown
Responia/
  Features/
    <FeatureName>/
      overview.md
      usecases.md
      specification-ux-ui.md
      technical-specification.md
      user-stories/
        <NN>-<descriptive-name>/
          <NN>-<descriptive-name>-userstory.md
        <NN>-<descriptive-name>/
          <NN>-<descriptive-name>-userstory.md
```

Never place user story files outside `user-stories/`.
Never create `user-stories/` in a parent or sibling folder.

Each user story must be placed in its own folder under `user-stories/`.
Each story folder name must start with an order number (`NN`) starting from `1` (zero-pad at least to 2 digits if you want stable lexicographic ordering).

## Source of Truth

Follow the repository instruction file for user stories **oraz powiązaną regułę**:

- `instructions/userstory.instructions.md`
- `.cursor/rules/userstory-documents.mdc`

Zawsze wczytaj **oba** te pliki przed generowaniem treści user stories.
If a `user-stories/template-userstory.md` file exists in the feature folder, use it only as a supplementary reference for style or structure — never copy template placeholder text into real stories.

## When to Use

Use this skill when the user asks to:

- write user stories for a feature
- create a `user-stories/` folder and populate it
- split a feature into user stories
- review whether the existing user stories are appropriately scoped

## Step 1: Resolve the Feature Folder

Before anything else, confirm the target feature folder.

Rules:

- existing feature folders must stay untouched in structure
- new `user-stories/` subfolder is created only if it doesn't already exist
- if the path is unclear, search under `Responia/Features/` and present the best candidates

Ask the user to confirm the folder before creating any files.

## Step 2: Read Existing Feature Documentation

Read all available documents inside the feature folder:

- `overview.md` — understand the problem and goals (business language, usually Polish)
- `usecases.md` — identify actors, triggers, and distinct workflows (business language, usually Polish)
- `specification-ux-ui.md` — understand the UX/UI shape and key views
- `technical-specification.md` — understand important technical constraints or flows if already present
- any short feature notes or analysis files if present

Also check `user-stories/` for existing stories to avoid duplication or accidental overwrites.

## Step 3: Analyse Scope and Propose Story Split

This is the most critical step. Do not skip it or jump to writing files.

### Scoping principles

- Each user story must deliver standalone business value — a user can receive value from it independently.
- Stories should be small enough to be completed in a few days, not two weeks.
- Do not artificially split stories just to create granularity — only split when there is a real boundary (different actor, different trigger, different value delivered).
- Do not merge stories when they serve clearly distinct actors or workflows.
- One use case does not always equal one story — a single use case may justify splitting if it has clearly separable sub-outcomes, or it may map to a single story if it is already focused.

### Heuristics for splitting

Split into multiple stories if:

- there are clearly distinct actors (e.g. end user vs admin)
- a workflow has a setup step and an operational step that could be delivered independently
- the feature has both a read and a write workflow that deliver distinct value
- the `usecases.md` already identifies separate named use cases with different triggers

Keep as a single story if:

- the feature is a focused capability with one actor and one clear outcome
- splitting would produce stories that are only meaningful together (not independently shippable)
- the scope is already small (a day or two of work)

### Proposal format

Before creating any files, present the proposed story split to the user:

```markdown
Proposed user stories for <Feature Name>:

1. <Story title> — <1-sentence rationale for why this is a separate story>
2. <Story title> — <1-sentence rationale>
...

Do you want to proceed with this split, or would you like to adjust the scope?
```

The numbers in the proposed list (`1..N`) define the delivery order. Use the same order numbers as `NN` when creating per-story folders.
Wait for user confirmation or adjustment before writing files.

## Step 4: Generate User Story Files

After the user confirms the split, create files following these rules.

### File naming

Each proposed story is assigned an order number (`1..N`) which represents the delivery order.
Create:

- Story folder: `<NN>-<descriptive-name>/` (inside `user-stories/`)
- Story file (inside the folder): `<NN>-<descriptive-name>-userstory.md`

Use zero-padding for `NN` (at least 2 digits) if you want stable ordering in the filesystem.

Use descriptive, kebab-case names ending in `-userstory.md` for the story file:

- `01-view-vulnerability-risks-userstory.md`
- `02-acknowledge-risk-userstory.md`
- `03-admin-configure-thresholds-userstory.md`

Avoid generic names like `story1-userstory.md` or `user-userstory.md` in the story file name.

### File format

Follow the `instructions/userstory.instructions.md` guidance exactly.

Each file must contain:

1. `## [Feature Name]` header — use the feature name, not the file name.
2. `**As a** / **I want** / **So that**` — specific role, capability, benefit.
3. `**Goal:**` — one sentence summary accessible to non-technical stakeholders.
4. `**Acceptance Criteria:**` — minimum 2, ideally 3–5 Given/When/Then criteria.
5. `**Notes:**` — optional; use for out-of-scope clarifications, related stories, or business context only.

### Acceptance criteria rules

- Cover the happy path first.
- Add at least one edge case or error scenario if it is realistic for this story.
- Each criterion must be independently testable.
- Do not include implementation details, API specs, or technical constraints — in Responia-Knowledge those belong in the feature’s technical documentation (e.g. `technical-specification.md`), not in user stories.

### What to avoid

- Do not include implementation details anywhere in the story.
- Do not write technical constraints in Notes — only business context belongs there.
- Do not copy template placeholder text into real files.
- Do not create a story that depends entirely on another story to deliver value.

## Step 5: Review and Iterate

After generating the files:

1. Summarize what was created and why each story boundary was chosen.
2. Identify the weakest acceptance criteria or the most ambiguous story.
3. Ask: "Are there edge cases or error scenarios missing from the acceptance criteria? Should any story be split further or merged?"

Do not close the task until the user is satisfied with scope and quality.

## Editing Rules

- Pisz user stories po **polsku**, zgodnie z regułą `.cursor/rules/userstory-documents.mdc` (zarówno treść, jak i nagłówki/sekcje).
- Preserve any existing user stories in the folder unless the user explicitly asks to replace them.
- Use the smallest set of file operations needed — do not recreate files that already exist and are already good.
- Never create or modify `overview.md`, `usecases.md`, `specification-ux-ui.md`, or `technical-specification.md` as part of this skill — those are owned by other skills (e.g. `create-feature`).

## Completion Criteria

The task is complete only when:

- the feature folder under `Responia/Features/` is confirmed
- the story split was proposed and approved by the user
- all agreed story folders exist in `user-stories/` with correct `<NN>-` numbering (starting from `1` in delivery order) and each folder contains exactly one `*-userstory.md` file
- all story files inside those folders must also start with the same `<NN>-` prefix (so folder and filename numbering match)
- each story follows the repo format with concrete, testable acceptance criteria
- edge cases and error scenarios are represented where realistic
- the user has confirmed quality or further adjustments have been applied

## Example Prompts

- Create user stories for the `1. Kalendarz Desktopowy` feature.
- Help me split this Responia feature into user stories based on the existing use cases.
- Is the scope of this feature right for one story or should we split it?
- Generate user story files for the feature at `Responia/Features/1. Kalendarz Desktopowy` in Responia-Knowledge.

