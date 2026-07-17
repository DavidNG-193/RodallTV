from __future__ import annotations

import hashlib
import json
import os
import shutil
from pathlib import Path
from typing import Any

from .config import Settings
from .models import Manifest, ManifestItem


class FileStore:
    def __init__(self, settings: Settings) -> None:
        self.settings = settings
        self.settings.runtime_dir.mkdir(parents=True, exist_ok=True)
        self.settings.content_dir.mkdir(parents=True, exist_ok=True)
        self.settings.staging_dir.mkdir(parents=True, exist_ok=True)

    @staticmethod
    def sha256(path: Path) -> str:
        digest = hashlib.sha256()
        with path.open("rb") as file:
            for block in iter(lambda: file.read(1024 * 1024), b""):
                digest.update(block)
        return digest.hexdigest().lower()

    def is_valid(self, path: Path, item: ManifestItem) -> bool:
        return (
            path.is_file()
            and path.stat().st_size == item.file_size_bytes
            and self.sha256(path) == item.hash_sha256
        )

    def active_file(self, item: ManifestItem) -> Path:
        return self.settings.content_dir / item.stored_file_name

    def staged_file(self, item: ManifestItem) -> Path:
        return self.settings.staging_dir / item.stored_file_name

    def clear_staging(self) -> None:
        if self.settings.staging_dir.exists():
            shutil.rmtree(self.settings.staging_dir)
        self.settings.staging_dir.mkdir(parents=True, exist_ok=True)

    def write_manifest_atomically(self, manifest: Manifest) -> None:
        payload: dict[str, Any] = {
            "hasAssignment": manifest.has_assignment,
            "playlistId": manifest.playlist_id,
            "playlistName": manifest.playlist_name,
            "playlistVersion": manifest.playlist_version,
            "currentDeviceVersion": manifest.current_device_version,
            "requiresSync": False,
            "items": [
                {
                    "playlistItemId": item.playlist_item_id,
                    "mediaId": item.media_id,
                    "position": item.position,
                    "originalFileName": item.original_file_name,
                    "storedFileName": item.stored_file_name,
                    "mediaType": item.media_type,
                    "mimeType": item.mime_type,
                    "fileSizeBytes": item.file_size_bytes,
                    "hashSha256": item.hash_sha256,
                    "customDurationSeconds": item.custom_duration_seconds,
                    "downloadUrl": item.download_url,
                }
                for item in manifest.items
            ],
        }
        temporary = self.settings.manifest_path.with_suffix(".json.tmp")
        temporary.write_text(json.dumps(payload, ensure_ascii=False, indent=2), encoding="utf-8")
        os.replace(temporary, self.settings.manifest_path)

    def load_active_manifest(self) -> dict[str, Any] | None:
        if not self.settings.manifest_path.is_file():
            return None
        return json.loads(self.settings.manifest_path.read_text(encoding="utf-8"))

    def activate_files(self, manifest: Manifest) -> int:
        for item in manifest.items:
            staged = self.staged_file(item)
            active = self.active_file(item)
            if staged.exists():
                os.replace(staged, active)

        required = {item.stored_file_name for item in manifest.items}
        deleted = 0
        for path in self.settings.content_dir.iterdir():
            if path.is_file() and path.name not in required:
                path.unlink()
                deleted += 1
        return deleted