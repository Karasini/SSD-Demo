# PR description

Generate a **Pull Request title** (max 5–6 words) and **description in English** from the branch diff against main and the content in `.cursor` (rules, commands, skills, etc.). Use **@Branch** as input — the diff of your branch against main.

---

## Output language and style

- **Output language:** English only. The PR title and all generated content (section titles, bullet points, summaries) must be in English.
- **Audience:** Engineers. Be concrete — facts, file paths, and changes only; no filler.
- **Template:** Always the same (see “Output format” below). Do not add or remove sections.
- **Visual representation:** If the changes can be shown clearly in a Mermaid diagram (e.g. pipeline step order, multi-step flow), add section 4 with a diagram. Otherwise, in section 4 write: “No visual representation — changes too small to illustrate.”
- **Format:** Raw Markdown only, ready to copy-paste (no code fence around the output, no extra text).

---

## Scope of analysis

1. **Diff (@Branch):** Only changes from the branch against main (new and modified files).
2. **Context:** Content of `.cursor` (rules, commands, skills, etc.) — project conventions and architecture.
3. **Goal:** Identify **significant** changes: new or changed application behaviour (features, bugfixes), refactors, API/domain impact, new concepts, **changes in the `.cursor` directory** (rules, commands, skills — anything under `.cursor/`), and deviations from those rules.

---

## Output format

The template is **fixed**. Fill each section; if nothing changed in a category, state it in one sentence (e.g. “No changes.”).

**Output:** Emit the result as **raw Markdown only**, ready to copy-paste into the PR title/description field. No code fence around the whole output; no extra commentary before or after. Just the Markdown content below, in English.

```markdown
## PR title
[Short title, max 5–6 words in English, summarizing the PR]

## PR description

### 1. Significant application behaviour changes
- [concrete points: what changes from user/business perspective — new or changed behaviour, effect of bugfixes; avoid excess technical detail]
- [if the diff has changes under `.cursor` (rules, commands, skills — any files under `.cursor/`), list them here; important for the team; if none, omit this bullet]

### 2. Deviations from norms (Cursor rules)
- [if any: new concepts, deviations from .cursor/rules (Architecture, FolderStructure, CodingStandards, etc.) — be specific; if none: “No deviations from norms.”]

### 3. Libraries / dependencies
- [if NuGet or other dependencies were added — list them and give a brief reason; if not — one sentence in English: “No libraries or dependencies were added in this PR.”]

### 4. Visual representation of changes
- [when it makes sense: a Mermaid diagram showing e.g. new or changed pipeline step order, multi-step flow; otherwise: “No visual representation — changes too small to illustrate.”]

### 5. PR author comment
<!-- Add your own comment for the reviewer or extra information here -->
```

---

## Requirements

1. Analyze **only** changes from @Branch (diff against main).
2. **PR title:** Generate a short title in English, **max 5–6 words**, summarizing the main change (e.g. feature name, bugfix scope, refactor area). Place it under “PR title” at the top of the output.
3. **“Significant application behaviour changes”:** Describe from business/user perspective — what changes in application behaviour (new features, bugfixes, changed outcome). Be concrete; avoid excess technical detail (file paths only when needed).
4. **“Deviations from norms (Cursor rules)”:** Deviations from `.cursor/rules` (Architecture, FolderStructure, CodingStandards, etc.). If none — one sentence: “No deviations from norms.”
5. **Changes in `.cursor`:** Always check the diff for changes in the **entire `.cursor` directory** (rules, commands, skills — any files under `.cursor/`) and list them in the “Significant application behaviour changes” section; important for the team.
6. **Libraries:** Check the diff for changes in `.csproj`, `packages`, `Directory.Packages.props`, etc. If none — state explicitly that no libraries or dependencies were added.
7. **“Visual representation of changes” (section 4):** When changes can be illustrated with a Mermaid diagram (e.g. changed call order in email processing / pipeline, multi-step flow) — add a diagram. When not needed (changes too small) — write: “No visual representation — changes too small to illustrate.”
8. **“PR author comment”:** Always leave as placeholder (`<!-- -->` block) — do not fill it with content.
9. Do not change the template structure (numbering, headings, section order).
10. **Copy-ready Markdown:** Output must be **raw Markdown only** — no wrapping in code blocks, no “here is your PR” text. The response body is the PR description; the user copies it as-is into GitHub/GitLab.
