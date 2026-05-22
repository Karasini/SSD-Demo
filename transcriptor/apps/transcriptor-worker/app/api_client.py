from typing import Any

import httpx

from app.config import settings


class ApiClient:
    def __init__(self) -> None:
        self._base = settings.api_base_url.rstrip("/")
        self._headers = {"X-Internal-Api-Key": settings.internal_api_key}

    async def patch_job(self, job_id: str, payload: dict[str, Any]) -> bool:
        """Patch job status. Returns False when the API responds with 404."""
        async with httpx.AsyncClient(timeout=60.0) as client:
            response = await client.patch(
                f"{self._base}/api/internal/v1/transcription-jobs/{job_id}",
                json=payload,
                headers=self._headers,
            )
            if response.status_code == 404:
                return False
            response.raise_for_status()
            return True
