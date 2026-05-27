## Source materials

| Material | Purpose | Link |
| --- | --- | --- |
| Feature overview | Business problem and goals | `./overview.md` |
| Feature use cases | Main user flows and expected support | `./usecases.md` |
| Existing transcription portal UI | Keep consistency with current screens and patterns | `../../1. Web transcription portal/specification-ux-ui.md` |

---

## Base layout

- **Transcript content area** - each line shows speaker label, start time, end time, and text.
- **Speaker management area** - user can rename speakers detected in this transcript.
- **Export action area** - export uses current speaker names and line time ranges.
- **Status and feedback area** - shows save success or error messages for speaker rename actions.

---

## Views / screen variants

### Transcript detail - default state

- The user sees transcript lines with speaker labels and time range (`start - end`).
- Generic labels can appear when the system cannot detect real names.
- The layout must stay readable for long transcripts.

### Transcript detail - renamed speakers state

- The user sees updated names after manual rename.
- All lines from the same detected speaker reflect the new name.
- The screen keeps the same line order and timing information.

### Transcript detail - export preview intent

- Before export, the user understands export will include speaker and time metadata.
- Exported text keeps the same speaker naming currently visible in the app.

---

## Components, cards, and visual variants

### Transcript line row

- **When:** For every transcript line.
- **How:** Show speaker label, start time, end time, and spoken text in one readable row or block.
- **After completion:** The user can quickly identify who speaks at each point in time.

### Speaker label

- **When:** Always visible for each line.
- **How:** Use a clear text style that separates label from transcript text.
- **After completion:** The user can scan conversation ownership without extra clicks.

### Time range metadata

- **When:** Always visible for each line.
- **How:** Show start and end time using one consistent format in all lines.
- **After completion:** The user can locate where each statement appears in the source audio/video.

### Rename speaker control

- **When:** User wants to replace a generic or incorrect speaker name.
- **How:** Provide inline edit or dedicated rename control for each detected speaker.
- **After completion:** New name appears in all related lines in the same transcript.

### Save feedback message

- **When:** Rename succeeds or fails.
- **How:** Show clear success/error message near the rename interaction.
- **After completion:** User understands whether changes are saved.

---

## Filters, search, and view controls

- No new filters are required in this feature.
- If transcript search already exists in the portal, it should continue to work with renamed speaker labels.
- Speaker rename should not reset current reading position in long transcripts.

---

## Additional panels / modals / drawers / details

### Rename speaker interaction

- Use a simple interaction pattern consistent with the portal (inline edit field or small modal).
- Show current speaker name before edit.
- Save action should validate non-empty value.
- Cancel action should keep previous value.

### Export interaction

- Keep existing export entry point from the transcript detail screen.
- Export output must include speaker label and start/end time for each line.

---

## UI states not visible in design

- **Loading transcript lines:** show loading state while speaker and time data is prepared.
- **No speaker data available:** show fallback label and explain that speaker detection was limited.
- **Rename validation error:** show message when the user tries to save an empty name.
- **Rename save error:** show retry option if save fails.
- **Long speaker names:** truncate in line view if needed, but keep full value accessible in rename input.
- **Very long transcripts:** keep smooth scrolling and stable rendering.

## Open questions for UX/UI

- Should rename use inline editing directly in the speaker list or a small modal dialog?
- Should the app warn when two speakers are renamed to the same display name?
- Should export allow optional exclusion of time metadata in future versions?
