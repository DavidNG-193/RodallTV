from __future__ import annotations

import logging
import signal
import time

import requests

from .api_client import AgentApiClient
from .config import load_settings
from .file_store import FileStore
from .logging_setup import configure_logging
from .player import MpvPlayer
from .sync_manager import SyncManager

logger = logging.getLogger(__name__)

_stop_requested = False


def request_stop(*_: object) -> None:
    """Solicita que el ciclo principal del agente termine."""
    global _stop_requested
    _stop_requested = True


def wait_until_next_cycle(seconds: int) -> None:
    """Espera sin impedir que Ctrl+C detenga el agente."""
    for _ in range(seconds):
        if _stop_requested:
            break

        time.sleep(1)


def main() -> None:
    settings = load_settings()
    configure_logging(settings.runtime_dir)

    signal.signal(signal.SIGINT, request_stop)
    signal.signal(signal.SIGTERM, request_stop)

    api = AgentApiClient(settings)
    store = FileStore(settings)
    sync_manager = SyncManager(api, store)
    player = MpvPlayer(settings)

    logger.info("Agente RodallTV iniciado.")

    while not _stop_requested:
        try:
            api.heartbeat()

            content_changed = sync_manager.synchronize()

            if content_changed:
                logger.info(
                    "Se detectó contenido nuevo. Reiniciando reproductor."
                )
                player.restart()
            else:
                # Si no hubo cambios, reproduce el contenido local ya existente.
                player.ensure_running()

        except requests.RequestException as exception:
            logger.warning(
                "Servidor no disponible; se conserva la reproducción local: %s",
                exception,
            )

            # Permite continuar reproduciendo sin conexión.
            player.ensure_running()

        except Exception:
            logger.exception(
                "Error no controlado en el ciclo del agente."
            )

            # No detener la reproducción válida ante otros errores.
            player.ensure_running()

        wait_until_next_cycle(settings.poll_interval_seconds)

    player.stop()
    logger.info("Agente detenido.")


if __name__ == "__main__":
    main()