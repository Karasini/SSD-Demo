## Use Case 1: Remove one transcription from history

**Actor:** User of the web transcription portal (same role as upload and read flows).

**Trigger:** The user no longer needs a job in the list (for example wrong file, old completed job, or failed attempt).

**What needs to be supported:**

- Open a job from the list or stay on its detail view and start **Remove** (or equivalent label).
- See a confirmation step that names the file (or shows how many items for bulk — see Use Case 3) and warns that removal cannot be undone.
- Confirm removal so the job disappears from the list and is no longer available by id.
- Remove the uploaded media and stored transcript data from the system (hard delete), not only hide the row in the UI.
- If the removed job was open in the detail panel, show a neutral state (for example “Select a transcription” or upload prompt), not a broken or empty detail for a missing job.
- See a clear error if removal fails (network or server) and keep the job in the list until the user retries or cancels the action.

## Use Case 2: Stop and remove a queued or in-progress job

**Actor:** Same user as Use Case 1.

**Trigger:** The user started a transcription by mistake, chose the wrong file, or does not want to wait for a running job.

**What needs to be supported:**

- Remove a job while status is **Queued** or **In progress** using the same remove flow as for finished jobs (one action: stop if needed, then delete).
- Stop further processing for that job on the server side so the worker does not keep working on a deleted job (or complete and write results for a job the user removed).
- Show the job as gone from the list after successful removal, even if processing had already started.
- Allow removal of other jobs in the list while one job is still running (same concurrent behavior as the portal today).
- If stop or delete partially fails, show a user-friendly message; do not leave the UI showing “removed” when the job still exists.

## Use Case 3: Remove several transcriptions at once (bulk delete)

**Actor:** Same user as Use Case 1.

**Trigger:** The user wants to clear many jobs (for example several failed runs or an old batch) without deleting one by one.

**What needs to be supported:**

- Enter a **selection mode** on the transcription list (for example via a “Select” or “Manage” control in the list header).
- Select multiple jobs with checkboxes (or equivalent multi-select on list rows).
- See how many jobs are selected before confirming.
- Run **Remove selected** (or equivalent) and see one confirmation that states the count and that removal cannot be undone.
- Remove all selected jobs in one confirmed action (hard delete for each), including a mix of statuses (queued, in progress, completed, failed) when the product allows removal for all statuses.
- Clear selection and exit selection mode after successful bulk remove, or when the user cancels.
- If bulk remove fails for some jobs, show which action failed in plain language; keep failed items in the list and selected where it helps the user retry.

## Use Case 4: Remove a failed or completed job without affecting others

**Actor:** Same user as Use Case 1.

**Trigger:** The user cleans up the list but still needs other transcriptions (including jobs still processing).

**What needs to be supported:**

- Remove only the chosen job(s); other jobs stay in the list with correct status.
- Continue to open, read, export, or upload while other jobs run, after removing unrelated jobs.
- See failed jobs removable like completed ones (no special “archive only” path).

## Use Case 5: Recover from remove or cancel errors

**Actor:** Same user as Use Case 1.

**Trigger:** Remove or cancel-and-delete fails (network, server error, or job already gone).

**What needs to be supported:**

- See a short, non-technical error message when removal fails.
- Keep the job visible in the list if it was not deleted.
- Allow the user to try remove again or dismiss the error and continue using the portal.
- Treat “job not found” after a successful delete elsewhere as success in the UI (refresh list) rather than a blocking error.

---

### Scope notes

| Item | In scope | Out of scope |
| --- | --- | --- |
| Single-job remove (all statuses) | Must-have | — |
| Cancel + remove queued / in progress | Must-have | — |
| Bulk delete with multi-select | Must-have | — |
| Confirmation before delete | Must-have | — |
| Hard delete (DB + object storage) | Must-have | Soft delete / archive |
| No login (same as portal) | Must-have | Per-user permissions |
| Audit log | — | Not required |
| Undo restore | — | Not required |
| Delete from detail only (no list selection) | Optional if list actions exist | List + detail both preferred |

**Open decision:** Exact label for the primary action — **Remove**, **Delete**, or **Cancel and remove** for in-progress jobs (UX may use one label for all statuses for simplicity).
