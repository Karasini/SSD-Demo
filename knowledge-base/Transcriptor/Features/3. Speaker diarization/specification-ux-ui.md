## Source materials

| Material | Purpose | Link / status |
| --- | --- | --- |
| `overview.md` | Problem and goals | `Transcriptor/Features/3. Speaker diarization/overview.md` |
| `usecases.md` | User flows and scope | Same folder |
| Web portal UX (base layout) | List + detail patterns, export, states | `Transcriptor/Features/1. Web transcription portal/specification-ux-ui.md` |
| Design file (Figma or equivalent) | Visual source of truth | **Not available yet** — this document is a UX/UI proposal for design |

**Proposal level:** Concrete enough for a first design pass on the **completed job detail** transcript area and rename flow. Extends the existing two-column portal; does not redesign the whole app.

---

## Base layout

No change to the overall **two-column** portal from Feature 1 (list left, workspace right). Speaker diarization mainly changes the **completed job detail** content in the right column.

- **Left column** — unchanged job list (file name, status, date).
- **Right column (completed job)** — replace the single plain **transcript body** with a **speaker-labeled transcript list** (see components below).
- **Header / export** — keep Download and Copy on completed jobs; exported content must include speaker labels and times (Use Case 4).

**Consistency:** Same status vocabulary as Feature 1. Do not show speaker-labeled rows while status is queued or in progress.

---

## Views / screen variants

### Job selected — completed (with speaker data)

- **When:** Job status is completed and the backend returned speaker segments.
- **Right column:** File name, completed status, **labeled transcript list**, export actions (Download, Copy).
- **Main rule:** Every spoken segment is one row (or block) with **speaker name**, **time**, and **text**.

### Job selected — completed (no speaker data)

- **When:** Job completed **before** this feature (legacy row with no segment data). New jobs always use diarization; failed diarization shows **Failed**, not this view.
- **Right column:** Short info message that speaker labels are not available; show **plain transcript text** as today (Feature 1 fallback).
- **Main rule:** Do not show empty speaker or time columns.

### Job selected — processing / queued / failed

- **When:** Same as Feature 1.
- **Right column:** Status and messaging only; **no** labeled transcript rows.
- **Failed:** No speaker export; recovery copy unchanged from Feature 1.

### Rename speaker (inline or panel)

- **When:** User chooses to rename a speaker from the transcript (Use Case 3).
- **How:** Design may use inline edit on the label, a small modal, or a side panel — TBD in design.
- **After save:** All rows with that speaker show the new name; brief success feedback (toast or inline).

---

## Components, cards, and visual variants

### Labeled transcript row

- **When:** Each segment in a completed job with diarization.
- **Shows:**
  - **Speaker label** — default from processing (for example “Person 1”); user-renamed value after edit.
  - **Time** — start time at minimum; start–end range if data allows (format example: `00:01:23` or `01:23 – 01:45` — design to pick one v1 format).
  - **Text** — what was said in that segment.
- **How:** Rows stacked vertically in reading order; comfortable line length and spacing between speakers optional (extra gap when speaker changes).
- **Variants:** Optional subtle color or icon per speaker index for quicker scanning (design); renamed names keep the same color mapping.

### Speaker label (default vs renamed)

- **When:** Always on each row.
- **How:** Renamed labels replace the default everywhere in that job for that speaker id (logical speaker), not only on one row.
- **Affordance:** Clear control to rename (for example click label, pencil icon, or “Rename speaker” on first row of that speaker — design to choose one pattern).

### Rename speaker flow

- **When:** User starts rename for one speaker identity in the job.
- **How:**
  - Show current default name and a text field for the new name.
  - Primary action **Save**; secondary **Cancel** discards changes.
  - Validate non-empty name and max length (suggest 1–64 characters — confirm in technical spec).
- **After save:** Update all rows for that speaker; show success feedback.
- **Error:** Plain message if save fails; keep previous names visible.

### Transcript body (fallback)

- **When:** Completed job without speaker data.
- **How:** Same as Feature 1 — scrollable plain text block.

### Export actions (updated behavior)

- **When:** Completed job with labeled transcript.
- **How:** Download and Copy produce text that includes **speaker**, **time**, and **spoken text** per line (exact line template in technical spec later; proposal: `[00:01:23] Anna: Hello everyone.` or similar).
- **When:** Fallback plain job — export unchanged from Feature 1 (plain text only).

---

## Filters, search, and view controls

**v1:** No filter-by-speaker or jump-to-speaker required.

**Later (optional):** Show only one speaker; search within transcript — out of v1 unless product expands scope.

---

## Additional panels / modals / drawers / details

### Rename speaker modal (if not inline)

- **When:** User confirms rename from row or speaker menu.
- **Shows:** Old name (read-only), new name field, Save, Cancel.
- **Behavior:** Focus in new name field; Enter saves if valid; Escape cancels.

**v1:** No separate “speaker management” screen listing all speakers — renaming from the transcript context is enough unless design adds a compact “Speakers in this job” list for discoverability (open question).

---

## UI states not visible in design

| State | When | Placement | Behavior |
| --- | --- | --- | --- |
| Labeled transcript loading | User opens completed job; segments fetching | Right column | Skeleton rows or spinner; do not flash plain text then relabel |
| Empty segments | Completed but zero lines (edge) | Right column | Plain message; offer export only if any text exists |
| Rename in progress | Save request running | Modal or inline | Disable Save; show loading on button |
| Rename validation error | Empty or too long name | Rename UI | Inline error; block Save |
| Rename save error | Network or server error | Rename UI + toast | Keep old names; allow retry |
| No diarization data | Legacy job only | Right column | Info banner + plain transcript fallback |
| Export with labels | Copy/Download on labeled job | Toolbar | Include speaker and time in output; success toast as Feature 1 |
| Single speaker | Engine detected one voice | Right column | Still show label + time per line, or one repeated label — confirm with product |

---

## Open questions for UX/UI

- **Time format:** Start only, or start–end per line? 12h vs 24h? Relative to file start (recommended)?
- **Rename entry point:** Edit on first occurrence only, on every speaker chip, or a “Speakers” list for the job?
- **Export format:** Single-line template for download/copy (needs alignment with technical spec).
- **Single-speaker files:** Show “Speaker 1” on every line or hide labels and show times only?
- **Visual distinction:** Color per speaker required in v1 or optional enhancement?
- **Audio sync:** Click timestamp to play from that point — confirm out of v1 (assumed yes).
