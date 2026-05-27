# Code review

Run a code review of branch changes (diff against main) using the rules from `.cursor/rules`. Use **@Branch** as input — the diff of your branch against main.

---

## Scope

- Review **all modified and new files** from the diff (@Branch).
- Evaluate against **all rules** from `.cursor/rules` (every `RULE.mdc` under `.cursor/rules/`). Include any new rules you add later — no need to update this command.
- Check that changes **follow patterns already used in the codebase** (handlers, endpoints, validators, commands/queries, repositories, tests, etc.). Flag inconsistencies with existing style and structure.

---

## Severity (required values)

Use **only** these values in the **Severity** column:

| Severity    | Meaning |
|------------|---------|
| **Critical** | Blocks merge: security flaw, data loss, broken API contract, layer violation (Features → Infra). |
| **High**     | Should fix: major architecture violation, missing validation/authorization, missing error handling, high bug risk. |
| **Medium**   | Recommended: minor convention violations, missing tests for new logic, magic strings, weak naming. |
| **Low**      | Optional: style consistency, missing docs, refactor suggestions. |
| **Suggestion** | Improvement idea, optimization, better API UX — not a defect. |

---

## Output Format

Each finding goes as **one row in the table**. Add a summary at the end.

### Results Table

```markdown
| # | Area | Rule | Severity | Description | How to Fix |
|---|------|------|----------|-------------|------------|
| 1 | …    | …    | …        | …           | …          |
```

- **#** — sequential number (1, 2, 3, …).
- **Area** — category (e.g. `Architecture`, `FolderStructure`, `CodingStandards`, `Validation`, `ExceptionHandling`, `Database`, `Tests`, `Endpoint`, `Handler`, `Security`). One word or short phrase.
- **Rule** — which `.cursor/rules` rule was violated. Use the rule name from `.cursor/rules/` (folder or `RULE.mdc`).
- **Severity** — one of: `Critical`, `High`, `Medium`, `Low`, `Suggestion`.
- **Description** — Short problem description, preferably with **file path** and **code snippet** (e.g. `Endpoint.cs:12` or type/method name).
- **How to Fix** — Brief, concrete steps or a reference to the rule plus what to change.

### At the end

```markdown
---
**Summary:** X Critical, Y High, Z Medium, W Low, V Suggestion.
```

---

## Requirements

1. Analyze **only changes from @Branch** (diff against main).
2. Reference the relevant rule from `.cursor/rules` in **How to Fix** when it helps.
3. Be specific: include file, line, type/method name when possible.
4. Do not duplicate the same issue in multiple rows — generalize or list all locations in one entry.
5. Do not add empty rows — the table should contain only real findings.

---

**Usage:** Attach **@Branch** to this command in chat (Cursor will use the diff of your branch against main).
