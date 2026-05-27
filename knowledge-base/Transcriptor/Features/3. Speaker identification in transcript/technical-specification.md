# Technical specification — Speaker identification in transcript

**Pattern:** Flow-Based Solution (Pattern 1)

**Monorepo root:** `transcriptor/`

| App | Path | Role |
| --- | --- | --- |
| Web UI | `apps/web/` | Render speaker lines, rename speakers, export with speaker + time |
| API | `apps/api-dotnet/` | Store transcript segments, store per-job speaker names, expose read/update endpoints |
| Worker | `apps/transcriptor-worker/` | Produce segments with speaker labels and start/end times |

**Builds on:** `Transcriptor/Features/1. Web transcription portal/technical-specification.md` and `Transcriptor/Features/2. Transcription removal/technical-specification.md`.

**v1 constraints (this feature):** speaker labels per line; start + end time per line; manual rename per transcript; export includes speaker + time; no global speaker mapping across transcripts; no merge/split speakers.

---

## Solution

Add structured transcript data to each completed job. The structured data contains line segments with `speakerId`, `speakerLabel`, `startSec`, `endSec`, and `text`. Keep `TranscriptText` for backward compatibility and quick plain-text rendering.

Speaker rename is a write action on one job. The API stores a per-job speaker map (`speakerId -> displayName`) and returns effective speaker labels in detail response. The worker still owns detection; the user owns final display names.

---

## API contracts (new and changed)

HTTP JSON uses camelCase on the wire.

### New contract: `TranscriptSegmentDto`

```json
{
  "speakerId": "spk_1",
  "speakerLabel": "Person 1",
  "startSec": 12.34,
  "endSec": 16.98,
  "text": "Hello and welcome everyone."
}
```

Rules:
- `startSec >= 0`
- `endSec > startSec`
- `speakerId` is stable inside one job
- `speakerLabel` is the effective label after rename resolution

### Change: `TranscriptionJobDetailDto`

Add fields:

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fileName": "meeting.mp3",
  "fileSizeBytes": 1048576,
  "contentType": "audio/mpeg",
  "status": "Completed",
  "createdAt": "2026-05-27T10:00:00Z",
  "updatedAt": "2026-05-27T10:20:00Z",
  "completedAt": "2026-05-27T10:20:00Z",
  "failureReason": null,
  "transcriptText": "Person 1 [00:12.34-00:16.98]: Hello and welcome everyone.",
  "detectedLanguage": "en",
  "segments": [
    {
      "speakerId": "spk_1",
      "speakerLabel": "Anna",
      "startSec": 12.34,
      "endSec": 16.98,
      "text": "Hello and welcome everyone."
    }
  ],
  "speakers": [
    {
      "speakerId": "spk_1",
      "displayName": "Anna",
      "defaultLabel": "Person 1"
    }
  ]
}
```

Notes:
- `segments` and `speakers` are optional (`null`) for non-completed jobs.
- `speakerLabel` in each segment must already include rename overrides.

### New endpoint: rename speaker

`PATCH /api/v1/transcription-jobs/{id}/speakers/{speakerId}`

Request:

```json
{
  "displayName": "Anna"
}
```

Success (`200`):

```json
{
  "speakerId": "spk_1",
  "displayName": "Anna",
  "defaultLabel": "Person 1"
}
```

Validation (`400`):

```json
{
  "title": "Validation failed",
  "status": 400,
  "errors": {
    "displayName": [
      "Display name is required."
    ]
  }
}
```

Not found (`404`):

```json
{
  "title": "Not Found",
  "status": 404,
  "detail": "Transcription job or speaker was not found."
}
```

Rules:
- Trim whitespace.
- Reject empty names after trim.
- Max display name length: 80.
- Rename is scoped to one job only.

### Internal worker callback change

Existing endpoint remains:
`PATCH /api/internal/v1/transcription-jobs/{id}`

Add optional payload fields for completed jobs:

```json
{
  "status": "Completed",
  "transcriptText": "Person 1 [00:12.34-00:16.98]: Hello and welcome everyone.",
  "detectedLanguage": "en",
  "segments": [
    {
      "speakerId": "spk_1",
      "speakerLabel": "Person 1",
      "startSec": 12.34,
      "endSec": 16.98,
      "text": "Hello and welcome everyone."
    }
  ]
}
```

If `segments` is missing or empty, API stores fallback one-speaker output:
- `speakerId = "spk_1"`
- `speakerLabel = "Speaker"`
- times from segment boundaries if available, otherwise `0` and `0`

---

## Data model changes (api-dotnet)

Extend `TranscriptionJob` persistence with:

```json
{
  "transcriptSegmentsJson": "[{\"speakerId\":\"spk_1\",\"speakerLabel\":\"Person 1\",\"startSec\":12.34,\"endSec\":16.98,\"text\":\"Hello and welcome everyone.\"}]",
  "speakerAliasesJson": "{\"spk_1\":\"Anna\"}"
}
```

Implementation notes:
- Add two nullable `text` columns in EF Core migration:
  - `TranscriptSegmentsJson`
  - `SpeakerAliasesJson`
- Keep JSON payload shapes stable in API contracts.
- Deserialize on read in query handler, then apply alias map to produce effective labels.

Why this shape:
- Minimal schema change in current project style.
- Keeps v1 simple (YAGNI: no extra normalized tables yet).
- Easy to migrate later to dedicated tables if needed.

---

## Feature flow

### Step 1 — Worker returns diarized segments

| | |
| --- | --- |
| **Caller** | `apps/transcriptor-worker` |
| **Endpoint** | `PATCH /api/internal/v1/transcription-jobs/{id}` |

1. Worker transcribes audio and builds ordered segments with `speakerId`, `speakerLabel`, `startSec`, `endSec`, and `text`.
2. Worker sends `Completed` callback with `transcriptText` and `segments`.
3. API validates payload and stores plain text + segments JSON.

Failure behavior:
- Invalid segment shape -> API responds `400`; worker maps this to failed callback and sends terminal `Failed` update.

### Step 2 — User opens job detail

| | |
| --- | --- |
| **Caller** | `apps/web` |
| **Endpoint** | `GET /api/v1/transcription-jobs/{id}` |

1. API reads job.
2. API builds effective speaker map:
   - default labels from segment data
   - override by `SpeakerAliasesJson` when present
3. API returns detail with `segments[]` and `speakers[]`.
4. Web renders each line with speaker name and `startSec-endSec`.

Failure behavior:
- Job not found -> `404`; UI shows existing not-found state.

### Step 3 — User renames speaker

| | |
| --- | --- |
| **Caller** | `apps/web` |
| **Endpoint** | `PATCH /api/v1/transcription-jobs/{id}/speakers/{speakerId}` |

1. Web sends new `displayName`.
2. API validates and updates alias map for this job.
3. API returns updated speaker item.
4. Web refreshes detail query and shows success message.

Failure behavior:
- Empty name -> `400`, keep current label.
- Unknown `speakerId` -> `404`, show retry message.

### Step 4 — Export with speaker + time

| | |
| --- | --- |
| **Caller** | `apps/web` |
| **Data source** | `GET /api/v1/transcription-jobs/{id}` result |

1. Web formats each segment line:
   - `{speakerLabel} [{start}-{end}]: {text}`
2. Copy and download use formatted text.
3. Renamed speaker labels are used because API already returns effective labels.

Example export line format:

```json
{
  "line": "Anna [00:12.34-00:16.98]: Hello and welcome everyone."
}
```

---

## Component responsibilities

### `apps/api-dotnet/`

- Add use case: `RenameSpeaker` inside `Features/TranscriptionJobs/`.
- Update `GetTranscriptionJobById` query mapping to include:
  - `segments`
  - `speakers`
- Update internal `UpdateTranscriptionJobStatus` handler to accept/store `segments`.
- Keep endpoint methods thin (validation + delegate to handlers).

### `apps/transcriptor-worker/`

- Update transcription pipeline to produce diarized segments.
- Keep existing plain `transcriptText` generation for compatibility.
- Ensure segment ordering by `startSec`.
- Keep safe fallback when diarization is unavailable.

### `apps/web/`

- Update `TranscriptionJobDetail` type with `segments` and `speakers`.
- Update job detail UI:
  - render segment list instead of plain `<pre>`
  - add rename controls
- Update export toolbar input from `transcriptText` to formatted segments text.
- Add mutation hook for rename endpoint and invalidate detail/list queries.

---

## API surface summary

| Method | Path | Auth | Purpose |
| --- | --- | --- | --- |
| `GET` | `/api/v1/transcription-jobs/{id}` | Public (v1) | Return transcript with speaker segments |
| `PATCH` | `/api/v1/transcription-jobs/{id}/speakers/{speakerId}` | Public (v1) | Rename one speaker for one job |
| `PATCH` | `/api/internal/v1/transcription-jobs/{id}` | Internal key | Worker callback with optional segments |

No change to list/create/delete endpoint URLs.

---

## Failure modes (caller-visible)

| Scenario | HTTP / UI behavior |
| --- | --- |
| Worker cannot produce diarization | Job still completes; API receives fallback one-speaker segments |
| Detail has no segments but has transcript text | UI falls back to plain transcript block |
| Rename with empty displayName | `400` validation error |
| Rename unknown speakerId | `404` problem details |
| Two speakers renamed to same display name | Allowed in v1; UI can show optional warning only |
| Export called before completion | Disabled in UI (same as current completed-only behavior) |

---

## Idempotency and consistency

| Step | Rule |
| --- | --- |
| Rename same value multiple times | Returns `200`; no extra side effects |
| Worker callback retried with same completed payload | Last write wins with same values (idempotent in effect) |
| Rename during detail polling | Query invalidation after mutation ensures eventual consistent view |

Ordering key is not required because this feature uses direct HTTP callbacks (no queue added).

---

## Out of scope (technical)

- Voice biometrics and verified real identity
- Cross-transcript speaker directory or reusable name dictionary
- Manual merge/split speaker clusters
- Real-time collaborative rename editing
- Dedicated export endpoint (client-side export remains)

---

## Open decision for implementation

Choose one diarization behavior for v1 in worker:

1. **Best effort diarization with fallback (recommended):**
   - If diarization model works, return real multi-speaker segments.
   - If it fails, still complete job with single-speaker segments.
2. **Strict diarization required:**
   - If diarization fails, mark job as `Failed`.

Recommendation: option 1, because transcript delivery remains reliable while still enabling speaker labels when available.
