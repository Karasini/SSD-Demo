# Web-based audio and video transcription tool
_Exported on 5/22/2026 at 15:07:27 GMT+2 from Cursor (3.5.17)_

---

**User**

/create-feature 

Goal

The goal of the project is to create a simple web-based tool for transcribing audio and video files.

The user should be able to upload a file through a web portal, start the transcription process, and then view the text result directly in the application.

Problem

Currently, transcribing audio/video files requires manually running CLI tools, online solutions are often expensive, managing input and output files, and manually opening the results.

There is a lack of a simple web interface that would allow users to:

* upload an audio or video file,
* view the processing status,
* browse a list of previous transcriptions,
* open and read transcription results in a clear and user-friendly format.

---

**Cursor**

Starting the create-feature workflow: I'll check how features are organized in this repo and which rules apply, then ask focused questions before we write anything.


Thanks for the clear starting point. This repo does not have a `Features/` tree yet, so we need to agree on location and fill a few gaps before writing `overview.md`, `usecases.md`, and `specification-ux-ui.md`.

---

## Proposed feature location

Unless you prefer another layout, I suggest:

```text
Features/1. Audio and video transcription/
  overview.md
  usecases.md
  specification-

---

**User**

continue

---

**Cursor**

Continuing the feature workflow: I'll check the transcriptor project and knowledge-base layout, then draft the feature docs from what we already have (using sensible defaults where details are still open).


Creating the feature folder and four documents from your brief, using `Transcriptor/Features/1. Web transcription portal/` in the knowledge-base (transcriptor repo is empty).


Created the feature documentation here:

**`knowledge-base/Transcriptor/Features/1. Web transcription portal/`**

| File | Status |
|------|--------|
| `overview.md` | Problem, goals, and explicit v1 out-of-scope |
| `usecases.md` | 5 use cases + v1 vs later table |
| `specification-ux-ui.md` | Two-column portal proposal (list + detail), states, open UX questions |
| `technical-specification.md` | Placeholder only (`TODO`) |

### Assumptions used (please confirm or correct)

1. **Location** — Docs live under **Transcriptor** in the knowledge-base; the `transcriptor` code repo is still empty.
2. **Actor** — Generic “user who needs a transcript” (researcher, editor, internal staff), not a specific persona.
3. **v1 scope** — Upload, status, history list, read result in-app. **Not** in v1: download, delete, cancel, search/filter, engine choice, billing.
4. **Layout** — Desktop: list left, upload/detail right; mobile behavior left as a design decision.
5. **Auth** — Marked as **open** (single-user vs login) in use cases and UX spec.

### Decisions that would sharpen the next docs

1. **Login for v1?** Open tool on a trusted network, or sign-in required?
2. **File rules** — Which formats and max file size?
3. **Concurrent jobs** — Can users start a second upload while one is running?
4. **Export** — Copy or download transcript in v1, or read-only in the browser?

If you answer these, I can update the three main docs and, when you want, run **create-technical-specification** for `technical-specification.md`. Next logical steps: **user stories** or **dev tasks** from this folder.

---

**User**

We don't need login for v1
File rules: max 2gb, common audio and video formats
Concurrent jobs -> yes
Export -> yes

---

**Cursor**

Updating the feature docs with your v1 decisions: no login, 2 GB / common formats, concurrent jobs, and export.


Updated all three feature docs with your v1 decisions.

### Recorded decisions

| Topic | v1 rule |
|--------|---------|
| **Login** | None — open app and upload |
| **Files** | Max **2 GB**; common audio/video (MP3, WAV, M4A, AAC, OGG, FLAC, MP4, MOV, WebM, MKV, etc.) |
| **Concurrent jobs** | Yes — new uploads while others run; list shows each job’s status |
| **Export** | **Download** (plain text) + **Copy** on completed detail view |

### What changed

- **`overview.md`** — New goals for export and concurrent jobs; v1 rules section; login removed from “open decisions”.
- **`usecases.md`** — Upload case includes 2 GB/format rules and concurrent jobs; **Use Case 6: Export**; scope table updated; only open item left: default transcription language (if the engine needs it).
- **`specification-ux-ui.md`** — No login in header; concurrent upload behavior; export toolbar on completed view; helper text and error copy for 2 GB limit; confirmed decisions moved out of open questions.

`technical-specification.md` is still `TODO`. Say if you want that filled next with **create-technical-specification**.

---

**User**

/create-technical-specification  let's use our internal guideline @knowledge-base/Dev Docs 

core assumptions for this project:

mono repo with structure:

transcription/
  apps/
    api-dotnet/
    web/
    transcriptor-worker/
  infrastructure/
  docs/

transcriptor-worker should use python fastAPI + whisperx https://github.com/m-bain/whisperX 

---

**Cursor**

Creating the technical specification: reading feature docs, Dev Docs guidelines, and the WhisperX reference.


Drafting the technical specification using Pattern 1 (flow-based), aligned with Dev Docs and the monorepo architecture.


Created **`technical-specification.md`** using **Pattern 1 (Flow-Based Solution)** and your Dev Docs conventions.

### Architecture (monorepo)

| Component | Stack | Role |
|-----------|--------|------|
| `apps/web/` | React + TypeScript | Portal, polling, client-side export |
| `apps/api-dotnet/` | .NET + PostgreSQL + EF Core | Jobs, uploads, storage, vertical slice `TranscriptionJobs` |
| `apps/transcriptor-worker/` | Python FastAPI + WhisperX | ffmpeg for video → WhisperX → plain text callback |
| `infrastructure/` | Docker Compose, MinIO/blob, Postgres | Local/prod wiring |

### Main flows documented

1. **List jobs** — `GET /api/v1/transcription-jobs`
2. **Upload + create** — `POST` multipart (2 GB, format whitelist) → `Queued` → trigger worker
3. **Worker run** — `POST /internal/v1/jobs/{id}/run` with signed media URL
4. **Callback** — `PATCH /api/internal/v1/transcription-jobs/{id}` (`InProgress` / `Completed` / `Failed`)
5. **Poll detail** — `GET .../{id}` every **3s** while active
6. **Export** — copy/download in browser from `transcriptText` (no extra API in v1)

### WhisperX (v1)

- Transcribe + optional align; **no diarization**
- Auto language detection
- Flatten segments to plain text for the portal

### Security (v1)

- No login; deploy behind VPN/allowlist; internal API key for worker ↔ API

### Open decisions (in the doc — please confirm)

1. **HTTP + callback** vs message queue for API → worker  
2. **Alignment on or off** (quality vs speed)  
3. **Whisper model** per environment (`large-v2` prod / `base` dev)

If you want changes (e.g. force a queue, server-side `.txt` download, or drop alignment), say which options you prefer and I’ll update the spec.

---

**User**

Rename Domain Model into API Contracts.
Then Let's add additional section about data model - show simple json examples, pascalCase and name it as Data Model.

---

**Cursor**

Renaming the section to "API Contracts" and adding a "Data Model" section with PascalCase JSON examples.


Updated `technical-specification.md`:

### Renamed
- **`## Domain model (API)`** → **`## API Contracts`**
  - Notes that HTTP JSON is **camelCase** on the wire; C# contract names are **PascalCase**
  - `TranscriptionJobStatus` enum + upload rules
  - DTO tables: `TranscriptionJobListItemDto`, `TranscriptionJobDetailDto`, list response, internal worker request/response

### Added
- **`## Data Model`**
  - `TranscriptionJob` persistence shape with **PascalCase** JSON examples: default (`Queued`), `Completed`, `Failed`
  - Field reference table (types, required, notes)
  - Status lifecycle diagram
  - Clarifies `StorageKey` is stored but not exposed on the public API

Feature flow examples are unchanged (camelCase HTTP payloads). Data Model shows the EF/PostgreSQL entity shape in PascalCase.

---

**User**

Implement this feature @knowledge-base/Transcriptor/Features/1. Web transcription portal  in transcriptor directory - keep in mind the @knowledge-base/Dev Docs 

---

**Cursor**

Delegating full implementation to a background agent using the feature spec and Dev Docs.


The web transcription portal is being implemented in the background against your feature spec and Dev Docs (`.NET API`, React web app, FastAPI/WhisperX worker, Docker Compose).

You’ll get a summary when it finishes, including how to run it locally and any gaps or shortcuts taken.

---

**User**

<system_notification>
The following task has finished. If you were already aware, ignore this notification and do not restate prior responses.

<task>
kind: subagent
status: success
task_id: 29c4eff9-cf42-440b-a691-1d98352cc2fc
title: Implement transcription portal
detail: <user_visible_high_level_summary>
The Web transcription portal monorepo is scaffolded under `transcriptor/` with a .NET 8 API, React portal, and FastAPI worker wired through Docker Compose. Upload → queue → mock transcription → completed transcript was verified end-to-end; WhisperX stays optional via `MOCK_MODE` (default) for dev without GPU.
</user_visible_high_level_summary>
<response>
## Summary

The **Web transcription portal** is implemented in `/Users/mawb/work/softwaremind/spec-driven-development/transcriptor/` as a monorepo with three apps and Docker Compose. End-to-end flow was verified: upload → `Queued` → `InProgress` → `Completed` with mock transcript text.

## Project tree (main artifacts)

```text
transcriptor/
├── README.md
├── .gitignore
├── docs/architecture.md
├── infrastructure/
│ └── docker-compose.yml
└── apps/
 ├── api-dotnet/
 │ ├── Dockerfile
 │ ├── nuget.config
 │ ├── Transcriptor.sln
 │ └── Transcriptor.Api/
 │ ├── Features/TranscriptionJobs/ # CQRS + endpoints
 │ ├── Domain/
 │ ├── Infrastructure/ # EF, MinIO, worker client
 │ └── Program.cs
 ├── web/
 │ ├── Dockerfile
 │ └── src/screens/transcription-portal/ # two-column UI
 └── transcriptor-worker/
 ├── Dockerfile
 └── app/ # FastAPI + mock/WhisperX
```

## How to run

```bash
cd /Users/mawb/work/softwaremind/spec-driven-development/transcriptor/infrastructure
docker compose up --build
```

| Service | URL |
|--------|-----|
| Portal | http://localhost:5173 |
| API | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger |

Local dev (split terminals): see root `README.md` for `dotnet run`, `uvicorn`, and `npm run dev`.

## What was implemented (v1)

| Area | Details |
|------|---------|
| **API** | `GET/POST /api/v1/transcription-jobs`, `GET` by id, `PATCH /api/internal/v1/transcription-jobs/{id}` with `X-Internal-Api-Key` |
| **Persistence** | PostgreSQL + EF Core migrations |
| **Storage** | MinIO (S3-compatible), presigned download URLs for worker |
| **Worker** | `POST /internal/v1/jobs/{jobId}/run`, idempotent run tracking, PATCH callbacks |
| **Web** | Two-column portal, upload zone, job list (newest first), detail + 3s polling for active jobs, copy/download `.txt` |
| **Concurrency** | Each upload creates a new job; background queue triggers worker per job |

## Open gaps / dev shortcuts

1. **Mock worker by default** — `MOCK_MODE=true` in Docker; no WhisperX/GPU in CI. Set `MOCK_MODE=false` and install `requirements-whisperx.txt` for real transcription.
2. **WhisperX image** — Worker Dockerfile is slim (ffmpeg only). GPU/CUDA images are not provided; document in README.
3. **No automated tests** — Spec did not require them; add unit/integration tests later.
4. **Worker port** — Not published on host (internal network only); intentional per spec.
5. **Content-Type** — `application/octet-stream` is allowed when the file extension is valid (some browsers send generic types).
6. **No rate limiting / auth** — As specified for v1; use VPN/reverse proxy in non-public deployments.
7. **No cancel/delete** — Out of scope for v1.

## Enable real WhisperX

```bash
# On worker
pip install -r requirements-whisperx.txt
export MOCK_MODE=false WHISPER_DEVICE=cpu WHISPER_COMPUTE_TYPE=int8 WHISPER_MODEL=base
```

Rebuild/restart the worker service with those env vars (and GPU settings if available).
</response>
</task>
</system_notification>
<user_query>The above subagent result is already visible to the user. DO NOT reiterate or summarize its contents unless asked, or if multi-task result synthesis is required. Otherwise end your response with a brief third-person confirmation that the subagent has completed. Don't repeat the same confirmation every time.</user_query>

---

**Cursor**

The background agent finished successfully; the transcription portal monorepo is in `transcriptor/` and ready to run with `docker compose up --build` from `infrastructure/`.

