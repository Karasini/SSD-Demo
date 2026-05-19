import asyncio
import logging
import tempfile
from pathlib import Path
from typing import Annotated
from uuid import UUID

import httpx
from fastapi import BackgroundTasks, FastAPI, Header, HTTPException
from pydantic import BaseModel, ConfigDict, Field

from app import warnings_filter

warnings_filter.configure()

from app.api_client import ApiClient
from app.config import settings
from app.transcription import map_failure_reason, transcribe_file

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = FastAPI(title="Transcriptor Worker")
api_client = ApiClient()

_active_jobs: set[str] = set()
_completed_jobs: set[str] = set()


class RunJobRequest(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    job_id: Annotated[UUID, Field(alias="jobId")]
    media_download_url: Annotated[str, Field(alias="mediaDownloadUrl")]
    file_name: Annotated[str, Field(alias="fileName")]
    content_type: Annotated[str, Field(alias="contentType")]


def _verify_api_key(api_key: str | None) -> None:
    if api_key != settings.internal_api_key:
        raise HTTPException(status_code=401, detail="Unauthorized")


async def _process_job(job_id: str, body: RunJobRequest) -> None:
    try:
        await api_client.patch_job(job_id, {"status": "InProgress"})

        if settings.mock_mode:
            await asyncio.sleep(settings.mock_delay_seconds)

        with tempfile.TemporaryDirectory() as tmp:
            media_path = Path(tmp) / body.file_name
            async with httpx.AsyncClient(timeout=600.0) as client:
                response = await client.get(body.media_download_url)
                response.raise_for_status()
                media_path.write_bytes(response.content)

            loop = asyncio.get_running_loop()
            transcript, language = await loop.run_in_executor(
                None, transcribe_file, media_path, body.file_name
            )

        await api_client.patch_job(
            job_id,
            {
                "status": "Completed",
                "transcriptText": transcript,
                "detectedLanguage": language,
            },
        )
        _completed_jobs.add(job_id)
    except Exception as exc:
        logger.exception("Job %s failed", job_id)
        reason = map_failure_reason(exc)
        try:
            await api_client.patch_job(
                job_id, {"status": "Failed", "failureReason": reason}
            )
        except Exception:
            logger.exception("Failed to report failure for job %s", job_id)
    finally:
        _active_jobs.discard(job_id)


@app.get("/health")
async def health() -> dict[str, str]:
    return {"status": "healthy", "mockMode": str(settings.mock_mode).lower()}


@app.post("/internal/v1/jobs/{job_id}/run", status_code=202)
async def run_job(
    job_id: UUID,
    body: RunJobRequest,
    background_tasks: BackgroundTasks,
    x_internal_api_key: str | None = Header(default=None),
) -> dict[str, str]:
    _verify_api_key(x_internal_api_key)
    job_key = str(job_id)

    if job_key in _active_jobs or job_key in _completed_jobs:
        return {"status": "accepted", "message": "Job already running or completed"}

    if body.job_id != job_id:
        raise HTTPException(status_code=400, detail="Job id mismatch")

    _active_jobs.add(job_key)
    background_tasks.add_task(_process_job, job_key, body)
    return {"status": "accepted"}
