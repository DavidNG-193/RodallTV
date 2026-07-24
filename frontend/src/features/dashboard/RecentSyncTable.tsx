import { Link } from "react-router-dom";
import type { Device } from "../../features/devices/devices.types";
import type { SyncLog } from "../../features/syncLogs/syncLogs.types";
import { formatDateTime } from "../../utils/fileFormatters";
import {
  getSyncResultClassName,
  getSyncResultLabel,
} from "../../utils/syncLogFormatters";

interface RecentSyncTableProps {
  logs: SyncLog[];
  devices: Device[];
}

export function RecentSyncTable({
  logs,
  devices,
}: RecentSyncTableProps) {
  const getDeviceName = (
    log: SyncLog,
  ): string =>
    log.deviceName ||
    devices.find(
      (device) => device.id === log.deviceId,
    )?.name ||
    "Dispositivo no disponible";

  return (
    <section className="panel">
      <div className="panel__heading dashboard-section-heading">
        <div>
          <h3>Sincronizaciones recientes</h3>
          <p>Últimos resultados reportados.</p>
        </div>

        <Link
          to="/sync-logs"
          className="dashboard-section-link"
        >
          Ver historial
        </Link>
      </div>

      {logs.length === 0 ? (
        <div className="empty-state">
          No hay sincronizaciones registradas.
        </div>
      ) : (
        <div className="table-wrapper">
          <table className="data-table">
            <thead>
              <tr>
                <th>Dispositivo</th>
                <th>Resultado</th>
                <th>Versión</th>
                <th>Fecha</th>
                <th>Descargados</th>
              </tr>
            </thead>

            <tbody>
              {logs.map((log) => (
                <tr key={log.id}>
                  <td>{getDeviceName(log)}</td>

                  <td>
                    <span
                      className={getSyncResultClassName(log.result)}
                    >
                      {getSyncResultLabel(log.result)}
                    </span>
                  </td>

                  <td>{log.syncedVersion}</td>
                  <td>{formatDateTime(log.startedAt)}</td>
                  <td>{log.downloadedFilesCount}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
