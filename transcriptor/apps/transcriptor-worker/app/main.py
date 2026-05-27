import app  # noqa: F401 — runs warnings_filter.configure() before ML imports

import asyncio
import logging
import tempfile
from contextlib import asynccontextmanager
from pathlib import Path
from uuid import UUID

import httpx
from fastapi import BackgroundTasks, FastAPI, Header, HTTPException
from pydantic import BaseModel, ConfigDict, Field

from app.api_client import ApiClient
from app.config import settings
from app.transcription import map_failure_reason, transcribe_file
from app.whisper_model import is_ready, wait_ready, warmup

logging.basicConfig(level=logging.INFO)
logging.getLogger("matplotlib").setLevel(logging.WARNING)
logging.getLogger("lightning").setLevel(logging.WARNING)
logging.getLogger("lightning.pytorch").setLevel(logging.WARNING)
logger = logging.getLogger(__name__)


@asynccontextmanager
async def lifespan(_app: FastAPI):
    logger.info("Warming up Whisper models at startup")
    loop = asyncio.get_running_loop()
    await loop.run_in_executor(None, warmup)
    yield


app = FastAPI(title="Transcriptor Worker", lifespan=lifespan)
api_client = ApiClient()

_active_jobs: set[str] = set()
_completed_jobs: set[str] = set()
_cancelled_jobs: set[str] = set()


class RunJobRequest(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    job_id: UUID = Field(validation_alias="jobId")
    media_download_url: str = Field(validation_alias="mediaDownloadUrl")
    file_name: str = Field(validation_alias="fileName")
    content_type: str = Field(validation_alias="contentType")


def _verify_api_key(api_key: str | None) -> None:
    if api_key != settings.internal_api_key:
        raise HTTPException(status_code=401, detail="Unauthorized")


def _is_cancelled(job_id: str) -> bool:
    return job_id in _cancelled_jobs


async def _process_job(job_id: str, body: RunJobRequest) -> None:
    try:
        if _is_cancelled(job_id):
            return

        if not await api_client.patch_job(job_id, {"status": "InProgress"}):
            logger.info("Job %s not found on API before processing; stopping", job_id)
            return

        if _is_cancelled(job_id):
            return

        with tempfile.TemporaryDirectory() as tmp:
            media_path = Path(tmp) / body.file_name
            async with httpx.AsyncClient(timeout=600.0) as client:
                response = await client.get(body.media_download_url)
                response.raise_for_status()
                media_path.write_bytes(response.content)

            if _is_cancelled(job_id):
                return

            loop = asyncio.get_running_loop()
            transcript, language, segments = await loop.run_in_executor(
                None, transcribe_file, media_path, body.file_name
            )

        if _is_cancelled(job_id):
            return

        if await api_client.patch_job(
            job_id,
            {
                "status": "Completed",
                "transcriptText": transcript,
                "detectedLanguage": language,
                "segments": segments,
            },
        ):
            _completed_jobs.add(job_id)
    except Exception as exc:
        if _is_cancelled(job_id):
            return
        logger.exception("Job %s failed", job_id)
        reason = map_failure_reason(exc)
        try:
            if not _is_cancelled(job_id):
                await api_client.patch_job(
                    job_id, {"status": "Failed", "failureReason": reason}
                )
        except Exception:
            logger.exception("Failed to report failure for job %s", job_id)
    finally:
        _active_jobs.discard(job_id)


@app.get("/health")
async def health() -> dict[str, str]:
    if not is_ready():
        raise HTTPException(
            status_code=503,
            detail="Whisper model is still loading",
            headers={"Retry-After": "30"},
        )
    return {
        "status": "healthy",
        "whisperModelReady": str(is_ready()).lower(),
    }


@app.post("/internal/v1/jobs/{job_id}/run", status_code=202)
async def run_job(
    job_id: UUID,
    body: RunJobRequest,
    background_tasks: BackgroundTasks,
    x_internal_api_key: str | None = Header(default=None),
) -> dict[str, str]:
    _verify_api_key(x_internal_api_key)
    job_key = str(job_id)

    if job_key in _cancelled_jobs:
        return {"status": "accepted", "message": "Job cancelled"}

    if job_key in _active_jobs or job_key in _completed_jobs:
        return {"status": "accepted", "message": "Job already running or completed"}

    if body.job_id != job_id:
        raise HTTPException(status_code=400, detail="Job id mismatch")

    loop = asyncio.get_running_loop()
    ready = await loop.run_in_executor(None, lambda: wait_ready(timeout=30.0))
    if not ready:
        raise HTTPException(
            status_code=503,
            detail="Whisper model is still loading",
            headers={"Retry-After": "30"},
        )

    _active_jobs.add(job_key)
    background_tasks.add_task(_process_job, job_key, body)
    return {"status": "accepted"}


@app.post("/internal/v1/jobs/{job_id}/cancel")
async def cancel_job(
    job_id: UUID,
    x_internal_api_key: str | None = Header(default=None),
) -> dict[str, str]:
    _verify_api_key(x_internal_api_key)
    job_key = str(job_id)
    _cancelled_jobs.add(job_key)
    _active_jobs.discard(job_key)
    return {"status": "cancelled"}
