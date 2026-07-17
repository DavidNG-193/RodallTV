from __future__ import annotations

import json
import logging
import subprocess
from pathlib import Path
from typing import Any

from .config import Settings

logger = logging.getLogger(__name__)


class MpvPlayer:
    def __init__(self, settings: Settings) -> None:
        self.settings = settings
        self.process: subprocess.Popen[str] | None = None

    def build_playlist_file(self) -> Path | None:
        manifest_path = (
            self.settings.runtime_dir / "active_manifest.json"
        )
        playlist_path = (
            self.settings.runtime_dir / "mpv_playlist.m3u"
        )
        content_dir = (
            self.settings.runtime_dir / "content"
        )

        if not manifest_path.exists():
            logger.warning(
                "No existe el manifiesto activo: %s",
                manifest_path,
            )
            return None

        try:
            with manifest_path.open(
                "r",
                encoding="utf-8",
            ) as manifest_file:
                manifest: dict[str, Any] = json.load(
                    manifest_file
                )

        except (OSError, json.JSONDecodeError) as exception:
            logger.error(
                "No fue posible leer el manifiesto activo: %s",
                exception,
            )
            return None

        items = manifest.get("items", [])

        if not isinstance(items, list) or not items:
            logger.warning(
                "El manifiesto activo no contiene elementos."
            )
            return None

        valid_files: list[Path] = []

        ordered_items = sorted(
            items,
            key=lambda item: item.get("position", 0),
        )

        for item in ordered_items:
            stored_file_name = item.get("storedFileName")

            if not stored_file_name:
                logger.warning(
                    "Un elemento no contiene storedFileName."
                )
                continue

            file_path = content_dir / stored_file_name

            if not file_path.exists():
                logger.warning(
                    "El archivo local no existe: %s",
                    file_path,
                )
                continue

            valid_files.append(file_path.resolve())

        if not valid_files:
            logger.warning(
                "No se encontraron archivos locales válidos."
            )
            return None

        try:
            playlist_path.parent.mkdir(
                parents=True,
                exist_ok=True,
            )

            with playlist_path.open(
                "w",
                encoding="utf-8",
                newline="\n",
            ) as playlist_file:
                playlist_file.write("#EXTM3U\n")

                for file_path in valid_files:
                    playlist_file.write(
                        f"{file_path.as_posix()}\n"
                    )

        except OSError as exception:
            logger.error(
                "No fue posible generar la playlist de mpv: %s",
                exception,
            )
            return None

        logger.info(
            "Playlist de mpv generada con %s archivos: %s",
            len(valid_files),
            playlist_path,
        )

        return playlist_path.resolve()

    def stop(self) -> None:
        if self.process is None:
            return

        if self.process.poll() is None:
            logger.info("Deteniendo mpv.")

            self.process.terminate()

            try:
                self.process.wait(timeout=5)

            except subprocess.TimeoutExpired:
                logger.warning(
                    "mpv no terminó a tiempo; se cerrará forzosamente."
                )
                self.process.kill()
                self.process.wait(timeout=5)

        self.process = None

    def restart(self) -> bool:
        playlist_path = self.build_playlist_file()

        if playlist_path is None:
            logger.warning(
                "No existe contenido válido para reproducir."
            )
            return False

        self.stop()

        command = [
            self.settings.mpv_path,
            f"--playlist={playlist_path}",
            "--loop-playlist=inf",
            "--image-display-duration=10",
            "--fullscreen",
            "--no-border",
        ]

        logger.info(
            "Iniciando mpv con la playlist: %s",
            playlist_path,
        )

        try:
            self.process = subprocess.Popen(
                command,
                text=True,
            )

            logger.info("mpv iniciado correctamente.")
            return True

        except FileNotFoundError:
            self.process = None

            logger.error(
                "No se encontró mpv. Revisa RODALL_MPV_PATH."
            )
            return False

        except OSError as exception:
            self.process = None

            logger.error(
                "No fue posible iniciar mpv: %s",
                exception,
            )
            return False

    def ensure_running(self) -> bool:
        if self.process is not None:
            if self.process.poll() is None:
                return True

            logger.warning(
                "mpv terminó inesperadamente. Se reiniciará."
            )
            self.process = None

        return self.restart()