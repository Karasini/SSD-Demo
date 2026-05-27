## Problem

Today the transcript text is hard to read when many people speak in one recording.

- The system does not clearly show which person said each line.
- Users cannot quickly follow a conversation or check who made a specific statement.
- The transcript view and exported transcript do not give clear speaker ownership for each line.
- Automatic labels like "Person 1" and "Person 2" are not useful enough for real work if users cannot rename them.

## Goals

The main goal is to make every transcript easier to understand by clearly linking each line to a speaker and to the time range when that speaker talked.

- Show a speaker label for every transcript line.
- Show start and end time for every line segment in the transcript.
- Keep speaker labels and time ranges visible in both the app view and exported output.
- Let users rename speaker labels manually in a simple way (for example change "Person 1" to "Anna").
- Save renamed speaker names so the same transcript keeps the updated names after reopen.

**Product rules (this release):** this is available to all users in the portal; no user-role restrictions.

**Out of scope for this feature:**

- Verified real-world identity detection (voice biometrics)
- Global speaker name mapping shared across different transcripts
- Manual merge or split of detected speakers
- Multi-user collaborative editing of speaker names
