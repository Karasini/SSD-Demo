## Use Case 1: Read transcript with clear speaker ownership
**Actor:** Portal user

**Trigger:** The user opens a finished transcript and needs to understand who said what.

**What needs to be supported:**

- Show each transcript line with a speaker label.
- Show start and end time for each line.
- Keep a clear order of lines so the user can follow the conversation flow.
- Keep speaker and time information easy to scan in long transcripts.

## Use Case 2: Rename detected speaker labels
**Actor:** Portal user

**Trigger:** The transcript contains generic labels such as "Person 1" and the user wants meaningful names.

**What needs to be supported:**

- Let the user edit a speaker label manually.
- Apply the new name to all lines that belong to that speaker in the same transcript.
- Prevent empty speaker names.
- Show clear feedback when the rename is saved.

## Use Case 3: Keep renamed speakers after reopen
**Actor:** Portal user

**Trigger:** The user reopens a transcript after renaming one or more speakers.

**What needs to be supported:**

- Load the latest saved speaker names for that transcript.
- Show the renamed labels in all transcript lines.
- Keep start and end time visible with the renamed speaker labels.

## Use Case 4: Export transcript with speaker and time metadata
**Actor:** Portal user

**Trigger:** The user exports or copies a transcript for sharing or analysis.

**What needs to be supported:**

- Include speaker labels in exported content.
- Include start and end time for each exported line.
- Use renamed speaker labels when the user changed them.
- Keep a stable and readable line format in export output.
