## Problem

The web transcription portal keeps every job in a growing list. Users cannot remove items they no longer need.

- Old, failed, or duplicate jobs stay in the list and make it harder to find the right transcription.
- A wrong file upload stays visible until someone clears it manually outside the app (not possible today).
- Sensitive media or transcript text may need to be removed from the system when work is done or a job was a mistake.
- A job that is still queued or running cannot be stopped and cleared from the portal.

## Goals

Users should control their transcription history. They can remove one or many jobs, stop work that is still running, and free storage used by uploaded files and results.

- Remove a single transcription from the list and from storage (hard delete).
- Remove several transcriptions at once (bulk delete).
- Stop and remove jobs that are still queued or in progress (cancel + delete).
- Remove completed, failed, and finished jobs the same way when the user no longer needs them.
- See a clear confirmation before removal so accidental deletes are less likely.
- After removal, the app shows a sensible next step (for example select another job or start a new upload).

**Product rules (same portal):** no user login; no audit log or undo in this release.

**Out of scope for this feature:**

- Audit trail of who deleted what and when
- Undo or restore after delete
- Search or filter (belongs to a separate list-improvement feature if added later)
- Admin-only or role-based delete rules
