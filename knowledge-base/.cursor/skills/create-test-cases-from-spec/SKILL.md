---
name: create-test-cases-from-spec
description: Generates user-oriented application test cases from feature documentation (overview, usecases, specification-ux-ui, technical-specification, user stories). Use when the user asks to prepare QA test cases, test scenarios, or test coverage for a feature.
---

# Create Test Cases From Spec

## Purpose

This skill prepares a complete, practical, **user-oriented** set of test cases for a feature using:

- `overview.md`,
- `usecases.md`,
- `specification-ux-ui.md`,
- `technical-specification.md`,
- `user-stories/*.md`.

The output should be easy to execute by any audience (QA, developer, PM, analyst), not only by technical specialists.

Language:
- **Default: Polish**.
- If the user explicitly asks for another language, follow the user's request.

---

## When to Use

Use this skill when the user asks for:

- "prepare test cases",
- "feature test scenarios",
- "test cases based on specification",
- "test coverage for user stories / UX / technical spec".

Do not use this skill when:

- the user only wants one quick ad-hoc test,
- there is no usable specification and requirements must be clarified first.

---

## Inputs and Required Files

Default source folder:

- `Responia/Features/<FeatureName>/`

Expected files:

- `overview.md`
- `usecases.md`
- `specification-ux-ui.md`
- `technical-specification.md`
- `user-stories/` (if present)

If some files are missing:

1. Continue with available artifacts.
2. Mark missing context as `"[to clarify]"` in a risks/gaps section.
3. Do not block test case generation.

---

## Output Location and Naming

Create artifacts in the feature folder:

- `Responia/Features/<FeatureName>/test-cases/`

## Default Output (required)

Create **one main file**:

- `test-cases.md`

This file is the source of truth and must be written in a user-oriented format.

## Optional Additional Files (only if user asks)

Create split files (e.g. API-only, UX-only, regression packs) **only when explicitly requested**:

- `test-cases-api.md`
- `test-cases-ux.md`
- `test-cases-regression.md`

Do not overwrite existing files without user confirmation.

---

## Workflow

Always follow these steps in order.

### 1) Scope Extraction

Collect requirements from documents and split them into:

- **Business flows** (overview + usecases),
- **UX behavior** (specification-ux-ui),
- **Contracts and integrations** (technical-specification),
- **Story acceptance intent** (user-stories).

### 2) Testability Breakdown

For each requirement define:

- preconditions,
- user actions / API-relevant interactions,
- expected result,
- success and failure criteria.

### 3) Generate User-Oriented Test Cases

For each critical area generate at minimum:

- 1 positive scenario,
- 1 negative scenario,
- 1 edge case scenario,
- 1 authorization/permission scenario (if applicable),
- 1 error handling and UX feedback scenario.

Rule for negative/edge variants:
- **P0/P1**: negative and edge variants are required.
- **P2**: variants are optional if risk is low; if omitted, add a short note why.

Treat this as a baseline, not a coverage target:

- `at least one` per test type is the minimum guardrail,
- add additional tests until risk is covered for the area,
- scale test depth based on complexity, criticality, and failure impact.

### 4) Add Traceability

Each test case must include references:

- `Źródło` / `Source`: `overview/usecases/specification-ux-ui/technical-spec/user-story`,
- `Requirement ID`: local identifier, e.g. `REQ-UX-03`,
- `Story`: e.g. `US-02` (if present).

### 5) Coverage Validation

Review and fill coverage gaps:

- whether each use case has at least one end-to-end test,
- whether each critical endpoint behavior has success + error coverage (from user perspective),
- whether each key UX state is covered (loading/empty/error/success),
- whether "no data", timeout, and retry/race scenarios are covered (if applicable).

### 6) Coverage Finalization

Finalize coverage only in terms of executable test cases.

If the user explicitly asks for automation mapping, derive it from existing test cases
(do not invent separate "required automation suggestions" by default).

---

## User-Oriented Test Case Template

Write each test in this format:

```markdown
### TC-<number>: <short user-facing title>

- Priorytet: P0 | P1 | P2
- Źródło: specification-ux-ui.md (sekcja X), technical-specification.md (sekcja Y), US-02
- Requirement ID: REQ-...
- Story: US-... | [to clarify]
- Warunki wstępne:
  - ...
- Kroki:
  1. ...
  2. ...
- Oczekiwany rezultat:
  - ...
- Wariant negatywny: (wymagany dla P0/P1, opcjonalny dla P2)
  - ...
- Wariant brzegowy: (wymagany dla P0/P1, opcjonalny dla P2)
  - ...
```

Important:
- Keep steps executable by a non-technical reader.
- Avoid internal architecture terms in main scenario steps.
- Mention API-level details only when necessary to execute or validate behavior.

---

## Recommended Structure of `test-cases.md`

1. `Jak używać tego dokumentu`
2. `Warunki wejściowe i dane testowe`
3. `Scenariusze testowe` (ordered by business flow, then risk)
4. `Kryteria zaliczenia`
5. `Luki / [to clarify]`

---

## Quality Bar

Test cases should be:

- atomic (one clear test objective),
- repeatable (clear steps and data),
- measurable (unambiguous expected result),
- maintainable (no duplication or excessive description),
- linked to requirements (traceability),
- understandable by mixed audience (QA/dev/product).

Avoid:

- tests with unclear expected results,
- duplicates of the same scenario,
- test descriptions without requirement source,
- over-technical language in the main user flow.

---

## Optional Enhancements

If requested, extend output with:

- `smoke/regression` split,
- release-oriented prioritization,
- automation mapping grouped by owner (`FE`, `BE`, `QA`) based on existing test cases only,
- dedicated test data and mocks section.

---

## Done Criteria

The skill is complete when:

1. `test-cases.md` exists in `test-cases/`.
2. Every critical use-case flow has at least one test.
3. Every key UX state has at least one test.
4. API-relevant success/error behavior is covered from user perspective.
5. Every test case includes traceability fields (`Źródło`/`Source`, `Requirement ID`, `Story` if present).
6. The document can be executed by a person without deep technical context.