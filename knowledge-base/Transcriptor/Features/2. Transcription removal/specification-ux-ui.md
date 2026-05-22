## Source materials

| Material | Purpose | Link / status |
| --- | --- | --- |
| `overview.md` | Problem and goals | `Transcriptor/Features/2. Transcription removal/overview.md` |
| `usecases.md` | Remove, cancel, bulk flows | Same folder |
| Portal UX (v1) | Layout, list, detail, status vocabulary | `Transcriptor/Features/1. Web transcription portal/specification-ux-ui.md` |
| Portal implementation | Existing structure to extend (no Figma) | `transcriptor/apps/web` — `transcription-portal.tsx`, `job-list-item.tsx`, `job-detail.tsx` |
| Design file (Figma) | Visual polish | **Not available** — proposal follows current portal patterns |

**Proposal level:** Concrete enough to implement against the existing two-column portal. Visual styling should match current list rows, badges, header, and primary button patterns.

---

## Base layout

No new page. Extend the existing **Transcriptor** portal layout:

- **Header** — unchanged: product title + **New transcription** primary button.
- **Left column — transcription list** — add list-header actions for selection mode and bulk remove; optional per-row remove when not in selection mode (design choice: detail-only remove is acceptable if list has bulk path).
- **Right column — job detail** — add **Remove** on the detail header for the selected job (all statuses).

**Consistency:** Same status badges and labels as today (Queued, In progress, Completed, Failed). Removal does not change status display for remaining jobs.

---

## Views / screen variants

### List — normal mode (default)

- **When:** User opens the app or exits selection mode.
- **Behavior:** Same as portal v1: click row selects job; checkboxes hidden.
- **New:** Optional overflow/remove control on row is **not** required if detail header provides single-job remove; bulk still needs selection mode.

### List — selection mode

- **When:** User taps **Select** (or **Manage**) in the list column header.
- **Behavior:**
  - Each row shows a **checkbox** (clicking checkbox toggles selection; clicking row body may still select for detail **or** only toggle checkbox — prefer checkbox does not navigate away during bulk select; row click toggles check when in selection mode).
  - List header shows: **Cancel** (exit selection mode), count **N selected**, and **Remove selected** (disabled when N = 0).
  - **Select all on screen** optional for v1; if omitted, user checks rows one by one.

### Job detail — with remove action

- **When:** A job is selected and detail is visible (processing, completed, or failed).
- **Header:** File name + status badge + **Remove** (text button or danger-styled control, right-aligned in `job-detail__header`).
- **Processing jobs:** Same header; remove uses cancel-and-delete behavior (Use Case 2); no separate “Cancel only” button required in v1.

### After remove — detail cleared

- **When:** User removed the job currently open in the right column (single or bulk including that id).
- **Right column:** Same neutral state as portal today when nothing is selected: **Select a transcription** / upload prompt; clear `selectedJobId`.
- **When:** User removed other jobs but kept the current selection — detail stays on the remaining selected job; list refreshes.

---

## Components, cards, and visual variants

### List column header (extended)

- **When:** Always visible above the scrollable list.
- **Shows:** Title **Transcriptions** (existing) + action cluster.
- **Normal mode actions:** **Select** (secondary) to enter selection mode.
- **Selection mode actions:** **Cancel** | **N selected** | **Remove selected** (primary danger or destructive secondary).

### List item — selection variant

- **When:** Selection mode active.
- **How:** Checkbox at start of row; file name, status badge, date unchanged; selected row keeps visible selected styling for checked items.
- **Long names:** Same truncation + `title` tooltip as today.

### List item — normal variant

- **When:** Default list; unchanged from portal except optional future icon slot.
- **How:** Full-width button row as today; no checkbox.

### Job detail header (extended)

- **When:** Detail visible for any job status.
- **How:** Flex row: title + badge left; **Remove** right.
- **Completed:** Export toolbar and transcript stay below header; remove does not replace export actions.

### Confirm dialog (remove)

- **When:** User chooses Remove (single) or Remove selected (bulk).
- **How:** Modal or native-styled dialog centered on viewport.
- **Copy (single):** Title **Remove transcription?** Body includes file name. Warning: **This cannot be undone.** Actions: **Cancel** (default focus) | **Remove** (destructive).
- **Copy (bulk):** Title **Remove N transcriptions?** Body: **This cannot be undone.** Actions: same pattern.
- **In progress / queued:** Same dialog; optional extra line: **Processing will stop.** (plain English, one sentence).

### Bulk remove progress (optional)

- **When:** Many jobs selected and API is slow.
- **How:** Disable **Remove selected** and show **Removing…** on the button or a light overlay on the list; avoid double submit.

---

## Filters, search, and view controls

No new filters or search for this feature. List sort stays **newest first** (portal v1).

Selection mode is a **view control** only: toggled from list header, not a permanent filter.

---

## Additional panels / modals / drawers / details

| Decision area | Rule |
| --- | --- |
| Confirm dialog | Required for every remove (single and bulk); no silent delete |
| Remove from detail | Opens same confirm dialog as list-driven remove for that job id |
| Remove from bulk | One dialog for the whole selection; no per-item dialog chain |
| Mobile / narrow | Selection mode and dialog work in stacked layout; list may be full width — same rules as portal responsive behavior |
| Upload in progress | Removing other jobs allowed; removing the job currently uploading is out of scope unless upload is tied to a created job id (confirm in technical spec) |

No settings drawer or audit history screen.

---

## UI states not visible in design

| State | When | Placement | Behavior |
| --- | --- | --- | --- |
| Remove idle | Detail open | Detail header | **Remove** enabled for all statuses |
| Confirm open | User clicked remove | Modal | Block second remove until closed |
| Removing (single) | After confirm | Dialog or detail | Primary action shows **Removing…**; disable close or cancel until done |
| Remove success | API OK | List + detail | Close dialog; toast optional (**Removed**); refresh list; clear detail if removed job was selected |
| Remove failed | API error | Dialog or inline | Show short message; keep job in list; re-enable **Remove** |
| Bulk partial failure | Some ids failed | List header or toast | Message like **Some transcriptions could not be removed**; refresh list; leave failed items selected if retry is offered |
| Selection empty | Selection mode, 0 checked | List header | **Remove selected** disabled |
| Exit selection | User taps **Cancel** | List | Clear all checks; return to normal mode |
| Deleted while viewing | Bulk removed includes open job | Right column | Neutral placeholder; no 404 flash |
| In progress removed | User removed running job | List | Row disappears; other in-progress rows unchanged |
| List empty after delete | Last job removed | Left column | Same empty state as portal: no transcriptions yet + upload CTA |

---

## Open questions for UX/UI

- **Row click in selection mode:** Toggle checkbox only vs still open detail — recommend **checkbox only** during selection mode to avoid accidental navigation.
- **Select all:** Include **Select all** in list header for bulk, or ship without it in first release.
- **Action label:** Single string **Remove** for all statuses vs **Cancel and remove** when status is in progress (product prefers one label if copy is clear in the dialog).
- **Toast vs inline success:** Portal today uses minimal feedback for export; align remove success with export (short toast) or list-only refresh.

**Confirmed:** No Figma; extend existing portal components; confirmation required; bulk delete in scope; hard delete; no audit UI.
