from __future__ import annotations

import logging
import signal
import time

import requests

from .api_client import AgentApiClient
from .config import load_settings
from .file_store import FileStore
from .logging_setup import configure_logging
from .models import PendingPowerCommand
from .player import MpvPlayer
from .power_manager import PowerManager
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


def handle_power_command(
    api_client: AgentApiClient,
    player: MpvPlayer,
    power_manager: PowerManager,
    command: PendingPowerCommand,
) -> bool:
    if not command.command_id or not command.command_type:
        logger.error("La orden de energía recibida es inválida.")
        return False

    if command.command_type not in {"Restart", "Shutdown"}:
        logger.error(
            "Comando de energía rechazado: %s",
            command.command_type,
        )
        return False

    logger.warning(
        "Preparando el dispositivo para: %s",
        command.command_type,
    )

    player.stop()
    api_client.acknowledge_power_command(command.command_id)
    api_client.close()
    power_manager.execute(command.command_type)

    return True


def main() -> None:
    settings = load_settings()
    configure_logging(settings.runtime_dir)

    signal.signal(signal.SIGINT, request_stop)
    signal.signal(signal.SIGTERM, request_stop)

    api = AgentApiClient(settings)
    store = FileStore(settings)
    sync_manager = SyncManager(api, store)
    player = MpvPlayer(settings)
    power_manager = PowerManager()

    logger.info("Agente RodallTV iniciado.")

    try:
        while not _stop_requested:
            try:
                heartbeat = api.heartbeat()

                if heartbeat.pending_power_command:
                    handled = handle_power_command(
                        api_client=api,
                        player=player,
                        power_manager=power_manager,
                        command=heartbeat.pending_power_command,
                    )

                    if handled:
                        return

                content_changed = sync_manager.synchronize()

                if content_changed:
                    logger.info(
                        "Se detectó contenido nuevo. "
                        "Reiniciando reproductor."
                    )
                    player.restart()
                else:
                    # Conserva la reproducción local cuando no hay cambios.
                    player.ensure_running()

            except requests.RequestException as exception:
                logger.warning(
                    "Servidor no disponible; se conserva la "
                    "reproducción local: %s",
                    exception,
                )

                player.ensure_running()

            except Exception:
                logger.exception(
                    "Error no controlado en el ciclo del agente."
                )

                player.ensure_running()

            wait_until_next_cycle(settings.poll_interval_seconds)

    finally:
        player.stop()
        api.close()
        logger.info("Agente detenido.")


if __name__ == "__main__":
    main()
