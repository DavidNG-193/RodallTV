from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path
import os
from uuid import UUID

from dotenv import load_dotenv

@dataclass(frozen=True)
class Settings:
    api_base_url: str
    device_uuid: UUID
    device_token: str
    agent_version: str
    poll_interval_seconds: int
    request_timeout_seconds: int
    runtime_dir: Path
    mpv_path: str

    @property
    def content_dir(self) -> Path:
        return self.runtime_dir / "content"

    @property
    def staging_dir(self) -> Path:
        return self.runtime_dir / "staging"

    @property
    def manifest_path(self) -> Path:
        return self.runtime_dir / "active_manifest.json"


def load_settings() -> Settings:
    load_dotenv()

    api_base_url = os.getenv("RODALL_API_BASE_URL", "").strip().rstrip("/")
    device_uuid_raw = os.getenv("RODALL_DEVICE_UUID", "").strip()
    device_token = os.getenv("RODALL_DEVICE_TOKEN", "").strip()

    if not api_base_url:
        raise ValueError("Falta RODALL_API_BASE_URL.")
    if not device_uuid_raw:
        raise ValueError("Falta RODALL_DEVICE_UUID.")
    if not device_token:
        raise ValueError("Falta RODALL_DEVICE_TOKEN.")

    poll_interval = int(os.getenv("RODALL_POLL_INTERVAL_SECONDS", "30"))
    timeout = int(os.getenv("RODALL_REQUEST_TIMEOUT_SECONDS", "30"))

    if poll_interval < 5:
        raise ValueError("RODALL_POLL_INTERVAL_SECONDS debe ser al menos 5.")
    if timeout < 1:
        raise ValueError("RODALL_REQUEST_TIMEOUT_SECONDS debe ser positivo.")

    return Settings(
        api_base_url=api_base_url,
        device_uuid=UUID(device_uuid_raw),
        device_token=device_token,
        agent_version=os.getenv("RODALL_AGENT_VERSION", "0.1.0").strip(),
        poll_interval_seconds=poll_interval,
        request_timeout_seconds=timeout,
        runtime_dir=Path(os.getenv("RODALL_RUNTIME_DIR", "runtime")).resolve(),
        mpv_path=os.getenv("RODALL_MPV_PATH", "mpv").strip(),
    )
