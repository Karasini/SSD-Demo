## Use Case 1: Upload a file and start transcription

**Actor:** User who needs a text transcript from an audio or video file (for example a researcher, content editor, or internal team member).

**Trigger:** The user has a media file on their computer and wants a text version without using command-line tools.

**What needs to be supported:**

- Select or drop one audio or video file for upload through the web portal.
- See which file is selected (name and, if available, size or type) before starting.
- Start the transcription job after upload is accepted.
- See a clear message if the file type or size is not allowed, with guidance on what is supported (max **2 GB**; common audio and video types such as MP3, WAV, M4A, AAC, OGG, FLAC, MP4, MOV, WebM, MKV).
- See confirmation that the job was created and transcription has started.
- Start a new upload while other jobs are already queued or in progress (concurrent jobs).

## Use Case 2: Follow transcription progress

**Actor:** Same user as Use Case 1.

**Trigger:** The user started a transcription job and wants to know whether it is still running, finished, or failed.

**What needs to be supported:**

- See status for the current or selected job: queued, in progress, completed, or failed.
- Distinguish “still working” from “ready to read” without refreshing the whole app manually (status updates in the UI).
- See a short explanation or next step when a job fails (for example retry upload or contact support), without technical error dumps as the only feedback.

## Use Case 3: Browse past transcriptions

**Actor:** Same user as Use Case 1.

**Trigger:** The user returns later or has several jobs and wants to find an earlier transcription.

**What needs to be supported:**

- See a list of previous transcription jobs (newest first is the default expectation).
- See each item’s file name (or label), status, and enough date or time context to tell jobs apart.
- Open one list item to view its detail (see Use Case 4).
- See an empty list state when no transcriptions exist yet, with a clear action to upload the first file.

## Use Case 4: Read a transcription result

**Actor:** Same user as Use Case 1.

**Trigger:** A transcription job completed successfully and the user wants to read the text.

**What needs to be supported:**

- Open a completed job from the list and see the full transcript text in the application.
- See which file the transcript belongs to (name or title) and that the job completed successfully.
- Read long transcripts comfortably (scrollable content, readable typography; exact styling is for UX/UI).
- See a clear state when a job is not finished yet (no full transcript, or placeholder with status) instead of an empty screen with no explanation.

## Use Case 5: Recover from upload or processing errors

**Actor:** Same user as Use Case 1.

**Trigger:** Upload fails, the server rejects the file, or transcription fails after the job started.

**What needs to be supported:**

- See a user-friendly error for failed upload (network, file too large, unsupported type).
- See failed jobs in the list with failed status, not as completed.
- Start a new upload after an error without losing access to other past jobs in the list.
- Reject files over **2 GB** or unsupported formats before or right after upload, with a clear reason.

## Use Case 6: Export a transcription result

**Actor:** Same user as Use Case 1.

**Trigger:** A transcription completed successfully and the user wants the text outside the browser (for example to paste into another tool or save as a file).

**What needs to be supported:**

- Download the transcript as a file (for example plain text) from the completed job detail view.
- Copy the full transcript text to the clipboard from the same view.
- See clear feedback when copy succeeds or download starts (and a user-friendly message if export fails).

---

### v1 scope notes

| Item | v1 | Later (optional) |
| --- | --- | --- |
| Upload + start job | Must-have | — |
| Status + history list + read result | Must-have | — |
| Concurrent jobs (multiple at once) | Must-have | — |
| Export (download + copy) | Must-have | — |
| No login | Must-have (v1) | User accounts / login |
| Max file size 2 GB; common audio/video formats | Must-have | — |
| Delete jobs from history | — | Optional |
| Cancel in-progress job | — | Optional |
| Search or filter the list | — | Optional |

**Open decision:** default language for transcription if the engine requires an explicit language setting.
