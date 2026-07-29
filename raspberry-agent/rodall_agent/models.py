from __future__ import annotations

from dataclasses import dataclass
from typing import Any


@dataclass(frozen=True)
class PendingPowerCommand:
    command_id: str
    command_type: str
    requested_at: str

    @classmethod
    def from_dict(
        cls,
        data: dict[str, Any],
    ) -> "PendingPowerCommand":
        return cls(
            command_id=str(data.get("commandId", "")),
            command_type=str(data.get("commandType", "")),
            requested_at=str(data.get("requestedAt", "")),
        )


@dataclass(frozen=True)
class HeartbeatResponse:
    device_id: str
    device_uuid: str
    device_name: str
    server_time_utc: str
    status: str
    current_playlist_version: int
    pending_power_command: PendingPowerCommand | None

    @classmethod
    def from_dict(
        cls,
        data: dict[str, Any],
    ) -> "HeartbeatResponse":
        pending_data = data.get("pendingPowerCommand")
        pending_command = (
            PendingPowerCommand.from_dict(pending_data)
            if isinstance(pending_data, dict)
            else None
        )

        return cls(
            device_id=str(data.get("deviceId", "")),
            device_uuid=str(data.get("deviceUuid", "")),
            device_name=str(data.get("deviceName", "")),
            server_time_utc=str(data.get("serverTimeUtc", "")),
            status=str(data.get("status", "")),
            current_playlist_version=int(
                data.get("currentPlaylistVersion", 0)
            ),
            pending_power_command=pending_command,
        )


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
