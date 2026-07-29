from __future__ import annotations

import unittest
from unittest.mock import patch

from rodall_agent.main import handle_power_command
from rodall_agent.models import HeartbeatResponse, PendingPowerCommand
from rodall_agent.power_manager import PowerManager


class _FakeApiClient:
    def __init__(self, events: list[str]) -> None:
        self._events = events

    def acknowledge_power_command(self, command_id: str) -> None:
        self._events.append(f"ack:{command_id}")

    def close(self) -> None:
        self._events.append("api:close")


class _FakePlayer:
    def __init__(self, events: list[str]) -> None:
        self._events = events

    def stop(self) -> None:
        self._events.append("player:stop")


class _FakePowerManager:
    def __init__(self, events: list[str]) -> None:
        self._events = events

    def execute(self, command_type: str) -> None:
        self._events.append(f"power:{command_type}")


class PowerCommandTests(unittest.TestCase):
    def test_heartbeat_parses_pending_command(self) -> None:
        heartbeat = HeartbeatResponse.from_dict(
            {
                "deviceId": "device-id",
                "deviceUuid": "device-uuid",
                "deviceName": "Lobby",
                "serverTimeUtc": "2026-07-29T20:00:00Z",
                "status": "Online",
                "currentPlaylistVersion": 3,
                "pendingPowerCommand": {
                    "commandId": "command-id",
                    "commandType": "Restart",
                    "requestedAt": "2026-07-29T19:59:30Z",
                },
            }
        )

        self.assertIsNotNone(heartbeat.pending_power_command)
        self.assertEqual(
            heartbeat.pending_power_command.command_type,
            "Restart",
        )

    def test_handler_uses_required_execution_order(self) -> None:
        events: list[str] = []
        command = PendingPowerCommand(
            command_id="command-id",
            command_type="Shutdown",
            requested_at="2026-07-29T19:59:30Z",
        )

        handled = handle_power_command(
            api_client=_FakeApiClient(events),
            player=_FakePlayer(events),
            power_manager=_FakePowerManager(events),
            command=command,
        )

        self.assertTrue(handled)
        self.assertEqual(
            events,
            [
                "player:stop",
                "ack:command-id",
                "api:close",
                "power:Shutdown",
            ],
        )

    def test_handler_rejects_arbitrary_commands(self) -> None:
        events: list[str] = []
        command = PendingPowerCommand(
            command_id="command-id",
            command_type="DeleteEverything",
            requested_at="2026-07-29T19:59:30Z",
        )

        handled = handle_power_command(
            api_client=_FakeApiClient(events),
            player=_FakePlayer(events),
            power_manager=_FakePowerManager(events),
            command=command,
        )

        self.assertFalse(handled)
        self.assertEqual(events, [])

    @patch("rodall_agent.power_manager.subprocess.run")
    def test_power_manager_uses_restricted_systemctl_command(
        self,
        run_mock,
    ) -> None:
        PowerManager.execute("Restart")

        run_mock.assert_called_once_with(
            ["sudo", "/usr/bin/systemctl", "reboot"],
            check=True,
        )

    def test_power_manager_rejects_unknown_command(self) -> None:
        with self.assertRaises(ValueError):
            PowerManager.execute("Arbitrary")


if __name__ == "__main__":
    unittest.main()
