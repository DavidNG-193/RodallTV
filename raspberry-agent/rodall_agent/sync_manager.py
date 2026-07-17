from __future__ import annotations

from datetime import datetime, timezone
import logging

from .api_client import AgentApiClient
from .file_store import FileStore
from .models import Manifest

logger = logging.getLogger(__name__)


class SyncManager:
    def __init__(self, api: AgentApiClient, store: FileStore) -> None:
        self.api = api
        self.store = store

    def synchronize(self) -> bool:
        started = datetime.now(timezone.utc)
        manifest: Manifest | None = None
        downloaded = 0
        deleted = 0

        try:
            manifest = self.api.get_manifest()

            if not manifest.has_assignment:
                self.api.report_sync(
                    result="NoChanges",
                    playlist_id=None,
                    synced_version=0,
                    started_at=started,
                    finished_at=datetime.now(timezone.utc),
                    message="El dispositivo no tiene playlist asignada.",
                    downloaded_files_count=0,
                    deleted_files_count=0,
                )
                return False

            if not manifest.requires_sync:
                self.api.report_sync(
                    result="NoChanges",
                    playlist_id=manifest.playlist_id,
                    synced_version=manifest.playlist_version or 0,
                    started_at=started,
                    finished_at=datetime.now(timezone.utc),
                    message=None,
                    downloaded_files_count=0,
                    deleted_files_count=0,
                )
                return False

            self.store.clear_staging()

            for item in manifest.items:
                active = self.store.active_file(item)
                if self.store.is_valid(active, item):
                    continue

                staged = self.store.staged_file(item)
                self.api.download(item.download_url, staged)

                if not self.store.is_valid(staged, item):
                    raise ValueError(
                        f"El archivo {item.original_file_name} no superó la validación de tamaño y SHA-256."
                    )
                downloaded += 1

            # Solo después de validar todos los archivos se activa la nueva versión.
            deleted = self.store.activate_files(manifest)
            self.store.write_manifest_atomically(manifest)
            self.store.clear_staging()

            self.api.report_sync(
                result="Success",
                playlist_id=manifest.playlist_id,
                synced_version=manifest.playlist_version or 0,
                started_at=started,
                finished_at=datetime.now(timezone.utc),
                message="Sincronización aplicada correctamente.",
                downloaded_files_count=downloaded,
                deleted_files_count=deleted,
            )
            logger.info("Playlist versión %s activada.", manifest.playlist_version)
            return True

        except Exception as exc:
            logger.exception("Falló la sincronización: %s", exc)
            self.store.clear_staging()
            try:
                self.api.report_sync(
                    result="Failed",
                    playlist_id=manifest.playlist_id if manifest else None,
                    synced_version=manifest.playlist_version or 0 if manifest else 0,
                    started_at=started,
                    finished_at=datetime.now(timezone.utc),
                    message=str(exc)[:1000],
                    downloaded_files_count=downloaded,
                    deleted_files_count=0,
                )
            except Exception:
                logger.exception("No fue posible reportar el fallo al servidor.")
            return False
