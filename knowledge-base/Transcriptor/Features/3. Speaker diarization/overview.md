## Problem

When people read a transcript from a meeting or interview, they often need to know **who said what**. Today the web portal shows one block of plain text. Every line looks the same, with no speaker name and no time for each part of the speech.

That creates real friction:

- The user must guess who is speaking or re-listen to the audio to match voices to text.
- Notes, quotes, and follow-up tasks are slower when the speaker is unclear.
- Generic labels from automatic tools (for example “Person 1”) are not useful until the user can map them to real names.

## Goals

The main goal is to make it obvious which transcript lines belong to which speaker, with time context for each spoken part. Users should also be able to fix speaker names when the system only provides placeholders.

- Show each transcript line (or segment) with a **speaker label** and **time information** so the reader can scan who spoke when.
- Keep speaker assignment **consistent** across the transcript (the same label always means the same speaker in that job).
- Let the user **rename** a speaker label (for example change “Person 1” to “Anna”) and see the new name everywhere that speaker appears in that transcription.
- Keep the experience aligned with the existing web portal (same job list and detail flow; no login required in this release).

**Out of scope for this feature:**

- Automatic recognition of real person names from voice (only system-generated labels plus manual rename).
- Editing the spoken **text** of the transcript (corrections, cuts, collaboration).
- Merging or splitting speakers when the engine assigned them wrong (unless added in a later release).
- Speaker profiles saved across jobs or user accounts.
- Mobile-native apps, billing, and choosing different transcription engines.
