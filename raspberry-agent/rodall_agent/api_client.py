from __future__ import annotations

from datetime import datetime, timezone
from pathlib import Path
from typing import Any

import requests
import logging
from datetime import datetime, timezone

from .config import Settings
from .models import Manifest


logger = logging.getLogger(__name__)

def to_utc_iso(value: datetime) -> str:
    if value.tzinfo is None:
        value = value.replace(tzinfo=timezone.utc)

    return (
        value.astimezone(timezone.utc)
        .isoformat(timespec="milliseconds")
        .replace("+00:00", "Z")
    )

class AgentApiClient:
    def __init__(self, settings: Settings) -> None:
        self._settings = settings
        self._session = requests.Session()
        self._session.headers.update({
            "X-Device-Id": str(settings.device_uuid),
            "X-Device-Token": settings.device_token,
            "Accept": "application/json",
        })

    def heartbeat(self) -> dict[str, Any]:
        response = self._session.post(
            f"{self._settings.api_base_url}/api/agent/heartbeat",
            json={"agentVersion": self._settings.agent_version},
            timeout=self._settings.request_timeout_seconds,
        )
        response.raise_for_status()
        return response.json()

    def get_assignment(self) -> dict[str, Any]:
        response = self._session.get(
            f"{self._settings.api_base_url}/api/agent/assignment",
            timeout=self._settings.request_timeout_seconds,
        )
        response.raise_for_status()
        return response.json()

    def get_manifest(self) -> Manifest:
        response = self._session.get(
            f"{self._settings.api_base_url}/api/agent/manifest",
            timeout=self._settings.request_timeout_seconds,
        )
        response.raise_for_status()
        return Manifest.from_dict(response.json())

    def download(self, url: str, destination: Path) -> None:
        destination.parent.mkdir(parents=True, exist_ok=True)
        with self._session.get(
            url,
            stream=True,
            timeout=self._settings.request_timeout_seconds,
        ) as response:
            response.raise_for_status()
            with destination.open("wb") as file:
                for chunk in response.iter_content(chunk_size=1024 * 1024):
                    if chunk:
                        file.write(chunk)

    def report_sync(
        self,
        *,
        result: str,
        playlist_id: str | None,
        synced_version: int,
        started_at: datetime,
        finished_at: datetime,
        message: str | None,
        downloaded_files_count: int,
        deleted_files_count: int,
    ) -> dict[str, Any]:
        payload = {
            "result": result,
            "playlistId": playlist_id,
            "syncedVersion": synced_version,
            "startedAt": to_utc_iso(started_at),
            "finishedAt": to_utc_iso(finished_at),
            "message": message,
            "downloadedFilesCount": downloaded_files_count,
            "deletedFilesCount": deleted_files_count,
        }
        response = self._session.post(
            f"{self._settings.api_base_url}/api/agent/sync-report",
            json=payload,
            timeout=self._settings.request_timeout_seconds,
        )
        if not response.ok:
            logger.error(
                "sync-report respondió HTTP %s. Respuesta: %s",
                response.status_code,
                response.text,
            )

        response.raise_for_status()
        return response.json()
