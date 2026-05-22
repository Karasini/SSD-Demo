# Technical specification — Transcription removal

**Pattern:** Flow-Based Solution (Pattern 1)

**Monorepo root:** `transcriptor/`

| App | Path | Role |
| --- | --- | --- |
| Web UI | `apps/web/` | Selection mode, confirm dialog, remove actions, list/detail refresh |
| API | `apps/api-dotnet/` | Delete orchestration, storage cleanup, worker cancel request |
| Worker | `apps/transcriptor-worker/` | Honour cancel for in-flight jobs; ignore callbacks for deleted jobs |
| Infra | `infrastructure/` | Unchanged Compose stack |

**Builds on:** `Transcriptor/Features/1. Web transcription portal/technical-specification.md` (same apps, contracts, and CQRS slice layout).

**v1 constraints (this feature):** hard delete (DB row + object storage object); single and bulk remove; cancel + remove for `Queued` / `InProgress`; no login; no audit log; confirmation is UI-only (not an API step).

---

## System overview

Removal is initiated only from **apps/web**. The API deletes persistence and storage and asks the worker to **cancel** when a job may still be processing. The worker stops treating cancelled jobs as active and does not write completion for a job the API has already removed.

```mermaid
sequenceDiagram
  participant Web as apps/web
  participant API as apps/api-dotnet
  participant DB as PostgreSQL
  participant Store as Object storage
  participant Worker as apps/transcriptor-worker

  Web->>API: DELETE job / POST bulk-delete
  alt status Queued or InProgress
    API->>Worker: POST cancel job
  end
  API->>Store: Delete object (storageKey)
  API->>DB: Delete job row
  API-->>Web: 204 or 200 bulk result

  Note over Worker: If still running, cancel flag set;<br/>callback 404 → discard
```

---

## Components and responsibilities

### `apps/api-dotnet/`

- Extend vertical slice `Features/TranscriptionJobs/`.
- **Writes:** `DeleteTranscriptionJobCommand`, `BulkDeleteTranscriptionJobsCommand` (+ handlers).
- **Reads:** unchanged (`GET` list/detail); deleted jobs no longer appear.
- **Infrastructure:**
  - `IObjectStorage` — add `DeleteAsync(storageKey)` (MinIO `RemoveObject`).
  - `ITranscriptionJobRepository` — add `RemoveAsync(TranscriptionJob)` or `DeleteByIdAsync`.
  - `TranscriptionWorkerClient` — add `CancelJobAsync(jobId)` → `POST internal/v1/jobs/{jobId}/cancel`.
- **UpdateTranscriptionJobStatusHandler:** if job row is missing, return `null` (`404`) — already true; worker must not recreate rows.

### `apps/transcriptor-worker/`

- In-memory sets: keep `_active_jobs`, `_completed_jobs`; add **`_cancelled_jobs`**.
- New endpoint: `POST /internal/v1/jobs/{job_id}/cancel` (internal API key).
- `_process_job`: before `InProgress` patch and before terminal patch, if `job_id in _cancelled_jobs`, exit without writing `Completed` / `Failed`.
- `run_job`: reject new runs for cancelled ids (same as completed).

### `apps/web/`

- **API client:** `deleteTranscriptionJob(id)`, `bulkDeleteTranscriptionJobs(ids)`.
- **Hooks:** `useDeleteTranscriptionJob`, `useBulkDeleteTranscriptionJobs` (TanStack Query mutations; invalidate `['transcription-jobs']` on success).
- **UI:** extend `transcription-portal.tsx` — selection mode state, list header actions, confirm dialog; `job-detail.tsx` — **Remove** in header; `job-list-item.tsx` — checkbox variant in selection mode.
- **Upload in progress:** no job id until `POST` create finishes — user cannot remove a file that is still uploading bytes (out of scope).

---

## API contracts (new and changed)

HTTP JSON uses **camelCase** on the wire. Examples below use camelCase.

### Bulk delete request — `BulkDeleteTranscriptionJobsRequest`

| Property | Type | Notes |
| --- | --- | --- |
| `ids` | `uuid[]` | Required; min 1; max **100** per request |

### Bulk delete response — `BulkDeleteTranscriptionJobsResponse`

| Property | Type | Notes |
| --- | --- | --- |
| `deletedIds` | `uuid[]` | Jobs fully removed |
| `failed` | `BulkDeleteFailureItem[]` | Jobs that could not be removed |

### `BulkDeleteFailureItem`

| Property | Type | Notes |
| --- | --- | --- |
| `id` | `uuid` | Job id |
| `reason` | `string` | User-safe short message (no stack traces) |

### Worker cancel response

```json
{
  "status": "cancelled"
}
```

Idempotent: second cancel for same id returns same shape.

---

## Feature flow

### Step 1 — Single remove (detail or one id)

| | |
| --- | --- |
| **Caller** | `apps/web` (after confirm dialog) |
| **Endpoint** | `DELETE /api/v1/transcription-jobs/{id}` |
| **Auth** | Public (v1; same as portal — deploy behind network controls) |

**Handler flow (write path):**

1. Load job by `id`. If missing → **`404`** (client may treat as success when refreshing list after a parallel delete).
2. If `status` is `Queued` or `InProgress` → call worker **`POST /internal/v1/jobs/{id}/cancel`** (best effort; continue delete even if worker unreachable — see failure modes).
3. Delete object at `storageKey` via `IObjectStorage.DeleteAsync` (log and continue if object already missing).
4. Remove row from `TranscriptionJobs` table.
5. Return **`204 No Content`**.

**Success:** no body.

**Not found (`404`):**

```json
{
  "title": "Not Found",
  "status": 404,
  "detail": "Transcription job was not found."
}
```

**Validation (`400`)** — not used for single delete by id.

---

### Step 2 — Bulk remove (selection mode)

| | |
| --- | --- |
| **Caller** | `apps/web` (after confirm with count) |
| **Endpoint** | `POST /api/v1/transcription-jobs/bulk-delete` |
| **Content-Type** | `application/json` |

**Request body:**

```json
{
  "ids": [
    "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "7c9e6679-7425-40de-944b-e07fc1f90ae7"
  ]
}
```

**Handler flow:**

1. Validate `ids` (non-empty, unique, count ≤ 100).
2. For each id, run the same delete steps as Step 1 inside a per-job try/catch (order within one job: cancel → storage → DB).
3. Collect `deletedIds` and `failed[]`.
4. Return **`200 OK`** with body (even when some fail — partial success).

**Response example (partial failure):**

```json
{
  "deletedIds": ["3fa85f64-5717-4562-b3fc-2c963f66afa6"],
  "failed": [
    {
      "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "reason": "Could not remove this transcription. Try again."
    }
  ]
}
```

**Validation (`400`):**

```json
{
  "title": "Validation failed",
  "status": 400,
  "errors": {
    "ids": ["At least one job id is required."]
  }
}
```

**Empty or duplicate ids:** normalize duplicates; reject empty array with `400`.

---

### Step 3 — API requests worker cancel

| | |
| --- | --- |
| **Caller** | `apps/api-dotnet` (inside delete handler) |
| **Endpoint** | `POST {WORKER_BASE_URL}/internal/v1/jobs/{jobId}/cancel` |
| **Auth** | Header `X-Internal-Api-Key: {shared secret}` |

**When:** job status is `Queued` or `InProgress` before storage/DB delete.

**Worker behavior:**

1. Add `jobId` to `_cancelled_jobs`.
2. Remove `jobId` from `_active_jobs` (allows a future re-upload with same id only if product recreates ids — ids are new GUIDs per job, so no reuse issue).
3. Return `200` with `{ "status": "cancelled" }`.

**Queued job never started on worker:** cancel is a no-op on worker; API still deletes storage and DB.

---

### Step 4 — Worker respects cancel and deleted jobs

| | |
| --- | --- |
| **Caller** | `apps/transcriptor-worker` (background task) |
| **Callback** | `PATCH /api/internal/v1/transcription-jobs/{jobId}` (existing) |

**Rules:**

- At start of `_process_job` and before terminal `PATCH`, if `job_id in _cancelled_jobs` → exit without callback (or callback only if needed for metrics — **prefer no callback**).
- If `PATCH` returns **`404`** (job deleted from API) → log at info; do not retry; discard result.
- Do not re-insert or update a job row after API deleted it.

**Race:** User deletes while worker is transcribing. Cancel + DB delete wins; late `Completed` callback must be ignored when row is gone (`UpdateTranscriptionJobStatusHandler` returns `null` → worker treats as done).

---

### Step 5 — Web UI refresh after remove

| | |
| --- | --- |
| **Caller** | `apps/web` |

**After single delete success (`204`):**

1. Invalidate `transcription-jobs` list query.
2. If removed id === `selectedJobId` → set `selectedJobId` to `null`, show neutral workspace (`Select a transcription` / upload).
3. Remove detail query for that id (or let `enabled: false` when id null).
4. Optional short toast: **Removed**.

**After bulk delete (`200`):**

1. Invalidate list.
2. If `selectedJobId` is in `deletedIds` → clear selection as above.
3. If `failed.length > 0` → show **Some transcriptions could not be removed**; keep failed ids checked in selection mode for retry.
4. If all selected ids in `deletedIds` → exit selection mode and clear checks.

**After delete error:**

- Keep job in list; show dialog/message; re-enable **Remove**.

**`404` on delete when job already gone:** treat as success — invalidate list, clear selection if needed (Use Case 5).

---

## API surface summary (additions)

| Method | Path | Auth | Purpose |
| --- | --- | --- | --- |
| `DELETE` | `/api/v1/transcription-jobs/{id}` | Public (v1) | Remove one job |
| `POST` | `/api/v1/transcription-jobs/bulk-delete` | Public (v1) | Remove many jobs |
| `POST` | `/internal/v1/jobs/{jobId}/cancel` | Internal key | Stop worker processing (worker host) |

Existing list/get/create/patch endpoints unchanged.

---

## CQRS mapping (api-dotnet)

| Operation | Type | Slice artifact |
| --- | --- | --- |
| Delete one job | Command | `DeleteTranscriptionJobCommand` + `DeleteTranscriptionJobHandler` |
| Bulk delete | Command | `BulkDeleteTranscriptionJobsCommand` + `BulkDeleteTranscriptionJobsHandler` |
| List / get | Query | Unchanged (read-only) |

Register new routes in `TranscriptionJobsEndpoints` / `EndpointsConfiguration.cs`.

---

## Frontend mapping (apps/web)

| UI area | API / behavior |
| --- | --- |
| Detail **Remove** | `DELETE /api/v1/transcription-jobs/{id}` after confirm |
| List **Select** / checkboxes | Local state only until **Remove selected** |
| **Remove selected** | `POST bulk-delete` with checked ids after confirm |
| Confirm dialog | Client-only; body shows file name or count; extra line for in-progress: **Processing will stop.** |
| List + detail refresh | `queryClient.invalidateQueries({ queryKey: ['transcription-jobs'] })` |
| Selection mode | **Cancel** clears checks and exits mode |
| Poll | Unchanged; deleted jobs disappear on next list fetch |

**New files (suggested):** `components/remove-confirm-dialog.tsx`, extend `job-list-item` for checkbox mode, list header toolbar in portal screen.

---

## Delete orchestration rules

| Order | Step | On failure |
| --- | --- | --- |
| 1 | Worker cancel (if active status) | Log warning; continue (avoid orphaned DB row) |
| 2 | Object storage delete | If not found, treat as success; other errors → fail this job |
| 3 | DB row delete | Fail this job if storage succeeded but DB fails (rare; surface in `failed[]`) |

**Hard delete:** no soft-delete column; no tombstone table in v1.

**Terminal jobs (`Completed` / `Failed`):** skip worker cancel; storage + DB only.

**Transcript text:** removed with the row (not a separate storage object in v1).

---

## Failure modes (caller-visible)

| Scenario | HTTP / UI |
| --- | --- |
| Delete unknown id | `404`; web may treat as success after refresh |
| Storage delete fails | Single: `500` or `503` with problem details; bulk: entry in `failed[]` |
| DB delete fails after storage removed | `500`; bulk: `failed[]`; ops may need manual storage cleanup (log `storageKey`) |
| Worker cancel unreachable | Delete continues; job row removed; worker may still run until callback `404` |
| Worker completes after user deleted | Callback `404`; no row recreated; user already sees job gone |
| Bulk: all fail | `200` with empty `deletedIds` and populated `failed` |
| Bulk: mixed | `200` partial; message **Some transcriptions could not be removed** |
| Network error on delete | Dialog error; job stays in list |
| Delete during upload (no job id yet) | Not applicable — disable remove until create returns |

---

## Idempotency and concurrency

| Step | Rule |
| --- | --- |
| `DELETE` same id twice | Second call → `404`; client treats as removed |
| Bulk duplicate ids in request | Dedupe server-side before processing |
| Parallel bulk + single on same id | One succeeds; other gets `404` or succeeds on already-deleted |
| Worker `run` after cancel | `202` but background task exits early if cancelled |
| Worker callback after delete | `404` → worker stops; no user-visible job |

---

## Security (v1)

Same as portal: no end-user auth. Delete endpoints are **public** at the API layer; restrict at network edge (VPN, allowlist) for non-public deployments. Rate-limit delete at gateway if abuse is a concern (optional).

---

## Out of scope (technical)

- Audit log table or delete-history API
- Undo / restore
- Soft delete / archive flag
- Per-user authorization
- `DELETE` with request body (use `POST bulk-delete` instead)
- Cascading delete of unrelated storage prefixes
- Removing a job while multipart upload is still in flight (no job id)

---

## Open decisions

| Topic | Recommendation | Alternative |
| --- | --- | --- |
| Bulk endpoint shape | **`POST .../bulk-delete`** with JSON body | `DELETE` collection with body (less client support) |
| Worker unreachable on cancel | **Continue delete** | Fail whole delete until worker ack |
| Max bulk size | **100** ids per request | Lower limit for stricter timeouts |
| Storage missing on delete | **Treat as success** | Fail delete |
| Select all in UI | Ship **without** select-all in v1 | Header **Select all** loads all ids from current list page only |

---

## Definition of done (implementation)

- Public `DELETE` and `POST bulk-delete` implemented and documented above.
- Worker `cancel` endpoint and cancelled-job checks in `_process_job`.
- `IObjectStorage.DeleteAsync` and repository remove wired in delete handlers.
- Web: selection mode, confirm dialog, detail remove, list refresh and selection clear.
- Partial bulk failure surfaced per `failed[]`.
- Manual test: remove completed job; remove in-progress job; bulk mix; delete already-deleted id returns `404` and UI recovers.
