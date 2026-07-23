import type { DeviceStatus } from "../../features/devices/devices.types";

interface StatusBadgeProps {
  status: DeviceStatus;
}

const labels: Record<DeviceStatus, string> = {
  Online: "En línea",
  Offline: "Desconectado",
  Syncing: "Sincronizando",
  Error: "Error",
  NotSynced: "Sin sincronizar",
};

export function StatusBadge({ status }: StatusBadgeProps) {
  return (
    <span className={`status-badge status-badge--${status.toLowerCase()}`}>
      {labels[status]}
    </span>
  );
}
