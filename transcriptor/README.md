# Transcriptor

Web transcription portal monorepo: React UI, .NET API, Python WhisperX worker.

## Structure

```text
transcriptor/
  apps/
    api-dotnet/          # .NET 8 API (PostgreSQL, MinIO, job orchestration)
    web/                 # React + TypeScript portal
    transcriptor-worker/ # FastAPI worker (WhisperX)
  infrastructure/        # docker-compose for local stack
  docs/
```

## Quick start (Docker)

From the repository root:

```bash
cd infrastructure
cp .env.example .env   # set HF_TOKEN (Hugging Face read token for speaker diarization)
docker compose up --build
```

| Service | URL |
| --- | --- |
| Web portal | http://localhost:5173 |
| API | http://localhost:8080 |
| API Swagger | http://localhost:8080/swagger |
| MinIO console | http://localhost:9001 (minioadmin / minioadmin) |

Upload an audio or video file in the portal. The worker transcribes with WhisperX (first startup downloads the model; CPU is slow but fine for local testing).

## Local development (without Docker)

### Prerequisites

- .NET 8 SDK
- Node.js 20+
- Python 3.11+
- PostgreSQL 16
- MinIO (or Docker only for postgres + minio)
- ffmpeg (for video inputs on the worker)

### API

```bash
cd apps/api-dotnet/Transcriptor.Api
dotnet run
```

Uses `appsettings.Development.json` (Postgres on localhost:5432, MinIO on localhost:9000).

### Worker

```bash
cd apps/transcriptor-worker
python -m venv .venv && source .venv/bin/activate
pip install -r requirements.txt
cp .env.example .env   # set HF_TOKEN
export API_BASE_URL=http://localhost:8080 INTERNAL_API_KEY=dev-internal-key
export HF_TOKEN=your-huggingface-read-token
export WHISPER_MODEL=base WHISPER_DEVICE=cpu WHISPER_COMPUTE_TYPE=int8
uvicorn app.main:app --reload --port 8000
```

### Web

```bash
cd apps/web
cp .env.example .env
npm install
npm run dev
```

Open http://localhost:5173 (Vite dev server). Set `VITE_API_BASE_URL=http://localhost:8080` in `.env`.

## Worker (WhisperX)

The worker warms up at startup (not on the first job): ASR + alignment for `WHISPER_WARMUP_LANGUAGES`.

**Startup time (CPU, `medium`):** ~2 minutes is normal. Rough breakdown:
- ~1 min — load ASR + pyannote VAD into RAM (no HTTP yet; looks like a “pause” in logs)
- ~10–40 s — alignment weights per language (parallel for `en,pl` when both are warmed up)

The Docker image **pre-downloads** weights at `docker compose build` (slow build once) so `docker compose up` restarts skip HTTP; loading into RAM on each container start still takes ~1 min on CPU. For faster local dev, use `WHISPER_MODEL=base`.

| Variable | Default | Notes |
| --- | --- | --- |
| `WHISPER_MODEL` | `base` | Docker Compose uses `medium` |
| `WHISPER_DEVICE` | `cpu` | `cuda` when GPU available |
| `WHISPER_COMPUTE_TYPE` | `int8` | `float16` on GPU |
| `WHISPER_WARMUP_LANGUAGES` | `en` | Comma-separated alignment languages to preload (e.g. `en,pl`) |
| `HF_TOKEN` | — | **Required.** Hugging Face read token with access to pyannote diarization models used by WhisperX |

Ensure `ffmpeg` is on `PATH` for video inputs. Accept the pyannote model terms on [huggingface.co](https://huggingface.co) before running diarization.

Every new job runs transcribe → align → **diarize**. Missing `HF_TOKEN` or diarization failure marks the job **Failed** (no plain-text-only completion).

Harmless startup noise (suppressed when possible): pyannote probing **torchcodec** in CPU Docker — it looks for CUDA libs (`libnvrtc`) and prints FFmpeg version tracebacks, but WhisperX uses **ffmpeg** for audio, not torchcodec.

After changing worker dependencies, rebuild the worker image:

```bash
cd infrastructure
docker compose build --no-cache worker
docker compose up -d worker
```

## API (v1)

| Method | Path | Description |
| --- | --- | --- |
| GET | `/api/v1/transcription-jobs` | List jobs (newest first) |
| GET | `/api/v1/transcription-jobs/{id}` | Job detail (segments + speakers when diarized) |
| POST | `/api/v1/transcription-jobs` | Multipart upload (`file`) |
| PATCH | `/api/v1/transcription-jobs/{id}/speakers/{speakerKey}` | Rename speaker display name |
| PATCH | `/api/internal/v1/transcription-jobs/{id}` | Worker callback (`X-Internal-Api-Key`) |

## Configuration

Shared secret for API ↔ worker: `InternalApi:ApiKey` / `INTERNAL_API_KEY` (default `dev-internal-key`).

See feature specs in the knowledge-base: `Transcriptor/Features/1. Web transcription portal/`, `3. Speaker diarization/`.
