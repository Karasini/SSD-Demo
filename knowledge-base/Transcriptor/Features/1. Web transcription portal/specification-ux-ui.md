## Source materials

| Material | Purpose | Link / status |
| --- | --- | --- |
| `overview.md` | Problem and goals | `Transcriptor/Features/1. Web transcription portal/overview.md` |
| `usecases.md` | User flows and v1 scope | Same folder |
| Design file (Figma or equivalent) | Visual source of truth | **Not available yet** — this document is a UX/UI proposal for design |
| Dev principles | Engineering conventions | `knowledge-base/Dev Docs/core-principles*.md` |

**Proposal level:** Concrete enough for a first design pass (layout, states, core screens). Visual branding and exact components come from design later.

---

## Base layout

Single-page web app with a persistent **two-column** layout on desktop; on narrow viewports the detail area may stack below the list or use a full-screen detail view (design to confirm).

- **Header** — product name or logo; optional global actions (for example “New transcription”). **No login** in v1 — user opens the app and can upload immediately.
- **Left column — transcription list** — scrollable list of jobs (history + in-progress). Primary navigation between jobs.
- **Right column — main workspace** — shows upload, in-progress status, or transcript detail depending on selection and job state.
- **Upload entry point** — visible when starting a new job: prominent in the right column and/or a primary button in the header or list header.

**Consistency:** One status vocabulary everywhere (queued, in progress, completed, failed) with matching labels and color or icon treatment in list and detail.

---

## Views / screen variants

### Default — no job selected

- **When:** User opens the app or has not picked a list item.
- **Right column:** Short welcome or instruction copy plus **upload zone** (drag-and-drop and “choose file” button).
- **Left column:** List of past jobs if any; otherwise empty state (see UI states).

### Upload in progress

- **When:** File is uploading before the job is created.
- **Right column:** Progress indicator for the active upload in this workspace.
- **Concurrent uploads:** User may start another job (new upload) while other jobs run; each new job appears in the list. Avoid blocking the whole app when one upload is in progress (design may use “New transcription” while list shows multiple in-progress items).
- **After upload:** Transition to “job created / transcription started” and show processing status (Use Case 2).

### Job selected — processing

- **When:** User selects a job that is queued or in progress.
- **Right column:** File name, status badge, simple progress or spinner, short line such as “Transcription in progress…”
- **Do not** show an empty transcript body without explanation.

### Job selected — completed

- **When:** User selects a completed job.
- **Right column:** File name, completed status, **scrollable transcript text** (main content), and **export actions** (Download, Copy). Optional metadata line (date completed, duration) if backend provides it.

### Job selected — failed

- **When:** User selects a failed job.
- **Right column:** Failed status, plain-language reason if available, suggested next step (upload again). No fake “success” styling.

---

## Components, cards, and visual variants

### List item (transcription job card / row)

- **When:** Each row in the left list.
- **Shows:** File name (truncate with tooltip if long), status badge, relative or absolute date/time.
- **How:** Click selects the job and loads the right column. Selected row has clear selected state (background or border).
- **Variants:** Status drives badge color/icon — in progress (animated or subtle pulse optional), completed, failed, queued.

### Upload zone

- **When:** New transcription flow on default view or explicit “New transcription”.
- **How:** Dashed border drop target; click opens file picker.
- **Helper text (v1):** Max size **2 GB**. Common formats, for example: MP3, WAV, M4A, AAC, OGG, FLAC, MP4, MOV, WebM, MKV (and similar common audio/video types).
- **After file chosen:** Show file name and size if available; primary button **Start transcription** (or equivalent).

### Transcript body

- **When:** Completed job detail.
- **How:** Monospace or readable sans-serif body text; comfortable line length; preserve paragraph breaks if the engine returns them.

### Export actions (completed jobs only)

- **When:** Job status is completed and transcript text is available.
- **How:** Toolbar or button row above or below the transcript: **Download** (saves plain text, filename derived from original file name where possible) and **Copy** (full transcript to clipboard).
- **After copy:** Brief success toast or inline “Copied”.
- **On failure:** Plain-language message; transcript remains visible in the app.

---

## Filters, search, and view controls

**v1:** No search or filter required. List sorted **newest first**.

**Later (optional):** Filter by status; search by file name — not in v1 unless product expands scope.

---

## Additional panels / modals / drawers / details

**v1:** No separate modal required if upload lives in the right column.

**Optional for design:**

- **Confirm before leaving** — only if upload is in progress and user navigates away (browser beforeunload or in-app warning).
- **Full-screen detail on mobile** — list → tap item → full-screen transcript with back to list.

No delete or settings drawer in v1 unless product adds them. Export uses inline actions on the detail view, not a separate drawer.

---

## UI states not visible in design

| State | When | Placement | Behavior |
| --- | --- | --- | --- |
| Empty list | No jobs yet | Left column | Illustration or icon optional; message: no transcriptions yet; CTA points to upload in right column |
| List loading | Initial fetch | Left column | Skeleton rows or spinner; right column neutral placeholder |
| Upload idle | Default new flow | Right column | Drop zone + file picker |
| Uploading | File transferring | Right column | Progress bar or spinner; cancel optional (product decision) |
| Upload error | Rejected type, over 2 GB, network | Right column | Inline alert; state max 2 GB and supported formats; keep drop zone usable |
| Export success | Download or copy OK | Detail toolbar | Toast or short confirmation |
| Export error | Download/copy failed | Detail toolbar | Error message; transcript still on screen |
| Multiple in progress | Several jobs running | Left list | Each row shows its own status; user can open any job or start another upload |
| Processing | Job running | Right column + list badge | Spinner or progress; optional auto-refresh or polling indicator |
| Completed detail | Job done | Right column | Full transcript scrollable |
| Failed detail | Job failed | Right column | Error message + link/button to start new upload |
| Long file name | Name overflows | List + detail | Truncate with ellipsis; full name in tooltip or detail header |
| Very long transcript | Many pages of text | Right column | Scroll only; no pagination required in v1 |

---

## Open questions for UX/UI

- **Polling vs push:** How often status refreshes in the UI (manual refresh button vs automatic updates).
- **Download file format:** Plain `.txt` default — confirm naming pattern (for example `interview.mp3` → `interview.txt`).
- **Branding:** Logo, colors, and design system — to be added when Figma exists.

**Confirmed for v1:** no login; max file size 2 GB; common audio/video formats; concurrent jobs allowed; export via download and copy on completed detail view.

Design reference: _TODO — add Figma link when available._
