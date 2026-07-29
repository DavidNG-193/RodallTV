from __future__ import annotations

import logging
import subprocess

logger = logging.getLogger(__name__)


class PowerManager:
    @staticmethod
    def execute(command_type: str) -> None:
        if command_type == "Restart":
            command = [
                "sudo",
                "/usr/bin/systemctl",
                "reboot",
            ]
        elif command_type == "Shutdown":
            command = [
                "sudo",
                "/usr/bin/systemctl",
                "poweroff",
            ]
        else:
            raise ValueError(
                f"Comando de energía no permitido: {command_type}"
            )

        logger.warning(
            "Ejecutando comando de energía: %s",
            command_type,
        )

        subprocess.run(
            command,
            check=True,
        )
