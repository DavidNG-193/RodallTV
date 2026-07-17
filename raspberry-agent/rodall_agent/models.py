from __future__ import annotations

from dataclasses import dataclass
from typing import Any


@dataclass(frozen=True)
class ManifestItem:
    playlist_item_id: str
    media_id: str
    position: int
    original_file_name: str
    stored_file_name: str
    media_type: str
    mime_type: str
    file_size_bytes: int
    hash_sha256: str
    custom_duration_seconds: int | None
    download_url: str

    @classmethod
    def from_dict(cls, data: dict[str, Any]) -> "ManifestItem":
        return cls(
            playlist_item_id=str(data["playlistItemId"]),
            media_id=str(data["mediaId"]),
            position=int(data["position"]),
            original_file_name=str(data["originalFileName"]),
            stored_file_name=str(data["storedFileName"]),
            media_type=str(data["mediaType"]),
            mime_type=str(data["mimeType"]),
            file_size_bytes=int(data["fileSizeBytes"]),
            hash_sha256=str(data["hashSha256"]).lower(),
            custom_duration_seconds=(
                int(data["customDurationSeconds"])
                if data.get("customDurationSeconds") is not None
                else None
            ),
            download_url=str(data["downloadUrl"]),
        )


@dataclass(frozen=True)
class Manifest:
    has_assignment: bool
    playlist_id: str | None
    playlist_name: str | None
    playlist_version: int | None
    current_device_version: int
    requires_sync: bool
    items: tuple[ManifestItem, ...]

    @classmethod
    def from_dict(cls, data: dict[str, Any]) -> "Manifest":
        items = tuple(
            sorted(
                (ManifestItem.from_dict(item) for item in data.get("items", [])),
                key=lambda item: item.position,
            )
        )
        return cls(
            has_assignment=bool(data.get("hasAssignment", False)),
            playlist_id=(str(data["playlistId"]) if data.get("playlistId") else None),
            playlist_name=data.get("playlistName"),
            playlist_version=(
                int(data["playlistVersion"])
                if data.get("playlistVersion") is not None
                else None
            ),
            current_device_version=int(data.get("currentDeviceVersion", 0)),
            requires_sync=bool(data.get("requiresSync", False)),
            items=items,
        )
