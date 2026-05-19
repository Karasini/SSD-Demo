# Transcriptor

Web transcription portal monorepo: React UI, .NET API, Python WhisperX worker.

## Structure

```text
transcriptor/
  apps/
    api-dotnet/          # .NET 8 API (PostgreSQL, MinIO, job orchestration)
    web/                 # React + TypeScript portal
    transcriptor-worker/ # FastAPI worker (WhisperX; mock mode by default)
  infrastructure/        # docker-compose for local stack
  docs/
```

## Quick start (Docker)

From the repository root:

```bash
cd infrastructure
docker compose up --build
```

| Service | URL |
| --- | --- |
| Web portal | http://localhost:5173 |
| API | http://localhost:8080 |
| API Swagger | http://localhost:8080/swagger |
| MinIO console | http://localhost:9001 (minioadmin / minioadmin) |

Upload an audio or video file in the portal. With default `MOCK_MODE=true`, the worker returns a mock transcript after ~3 seconds.

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
export MOCK_MODE=true API_BASE_URL=http://localhost:8080 INTERNAL_API_KEY=dev-internal-key
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

## Real WhisperX transcription

Default **local dev** uses mock mode (no GPU, no model download). The Docker stack in `infrastructure/docker-compose.yml` builds the worker with WhisperX when `INSTALL_WHISPERX=true` (already set there) and runs with `MOCK_MODE=false`.

To enable WhisperX locally (without Docker):

1. Install worker extras: `pip install -r requirements-whisperx.txt`
2. Set environment variables:
   - `MOCK_MODE=false`
   - `WHISPER_MODEL=base` (or `large-v2` for production)
   - `WHISPER_DEVICE=cuda` or `cpu`
   - `WHISPER_COMPUTE_TYPE=float16` or `int8` (CPU)
3. Ensure `ffmpeg` is on `PATH`.

After changing worker dependencies, rebuild the worker image:

```bash
cd infrastructure
docker compose build --no-cache worker
docker compose up -d worker
```

The first transcription job downloads models inside the container; CPU mode is slow but works for local testing.

To use mock mode in Docker instead, set `INSTALL_WHISPERX: "false"` under `worker.build.args` and `MOCK_MODE: "true"` in `worker.environment`.

GPU images and CI are out of scope for v1; use mock mode in pipelines and test WhisperX on a GPU machine.

## API (v1)

| Method | Path | Description |
| --- | --- | --- |
| GET | `/api/v1/transcription-jobs` | List jobs (newest first) |
| GET | `/api/v1/transcription-jobs/{id}` | Job detail |
| POST | `/api/v1/transcription-jobs` | Multipart upload (`file`) |
| PATCH | `/api/internal/v1/transcription-jobs/{id}` | Worker callback (`X-Internal-Api-Key`) |

## Configuration

Shared secret for API ↔ worker: `InternalApi:ApiKey` / `INTERNAL_API_KEY` (default `dev-internal-key`).

See feature specs in the knowledge-base: `Transcriptor/Features/1. Web transcription portal/`.
