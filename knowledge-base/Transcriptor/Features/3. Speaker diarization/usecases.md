## Use Case 1: Read a transcript with speaker labels and times

**Actor:** User who reads meeting or interview transcripts in the web portal (for example a researcher, journalist, or team member).

**Trigger:** A transcription job completed successfully and the user opens it to understand the conversation.

**What needs to be supported:**

- See the transcript as a sequence of lines or blocks, not only one plain paragraph.
- See a **speaker label** on each line (or block) so it is clear who said that text.
- See **time information** for each line (for example when that speaker started, or a start–end range — exact format is for UX/UI).
- Scan long transcripts comfortably (scrollable content, readable typography).
- See which file or job the transcript belongs to, same as today.

## Use Case 2: Distinguish speakers at a glance

**Actor:** Same user as Use Case 1.

**Trigger:** The transcript has two or more speakers and the user needs to follow the dialogue quickly.

**What needs to be supported:**

- Tell different speakers apart without reading every word (for example different label styling or grouping — exact presentation is for UX/UI).
- See the same speaker label repeat for all lines from that speaker in one job.
- Handle a **single-speaker** recording in a clear way (for example one label for all lines, or a message that only one speaker was detected — product/UX to confirm).

## Use Case 3: Rename a speaker label

**Actor:** Same user as Use Case 1.

**Trigger:** The transcript uses generic names (for example “Person 1”, “SPEAKER_00”) and the user knows the real names.

**What needs to be supported:**

- Change one speaker’s display name to a name the user chooses (for example “Person 1” → “Anna”).
- Apply the new name to **all lines** in that job that used the old label for that speaker.
- See the updated labels immediately in the transcript view after save.
- Enter a sensible name (non-empty; reasonable max length — exact rules in UX/UI and technical spec later).
- Cancel or abandon an edit without saving, when the UI offers that flow.

## Use Case 4: Export a transcript with speakers and times

**Actor:** Same user as Use Case 1.

**Trigger:** The user wants the labeled transcript outside the browser (file or clipboard).

**What needs to be supported:**

- Download a text file that includes speaker labels and time information per line, not only raw text.
- Copy the full labeled transcript to the clipboard with the same structure as download (unless product decides otherwise).
- Keep export available only when the job completed successfully and labeled content exists.
- See clear feedback when copy or download succeeds or fails.

## Use Case 5: Open a legacy job without speaker data

**Actor:** Same user as Use Case 1.

**Trigger:** The user opens a completed job that was created **before** speaker diarization existed (no segment data stored). New jobs always run diarization; if it fails, the job status is **Failed**, not completed without labels.

**What needs to be supported:**

- Still read the transcript text (fallback to plain or lightly formatted text).
- See a short, plain-language explanation that speaker labels are not available for this job (not a technical error dump).
- Avoid showing broken or empty speaker columns when data is missing.

## Use Case 6: Follow job status while diarization runs

**Actor:** Same user as Use Case 1.

**Trigger:** The user uploaded a file and the job includes speaker detection as part of processing.

**What needs to be supported:**

- See the same job statuses as today (queued, in progress, completed, failed) while processing runs.
- Not show speaker-labeled lines until the job is completed and data is ready.
- See the in-progress state in the detail view instead of an empty labeled transcript with no explanation.

---

**v1 must-have:** Use Cases 1–4 and 6.

**v1 should support:** Use Case 5 (fallback for jobs without speaker data).

**Later (optional):** Filter or jump to one speaker only; click a timestamp to play audio; merge two speaker labels when the engine split one person.
