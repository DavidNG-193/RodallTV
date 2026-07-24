import axios from "axios";
import { RefreshCw } from "lucide-react";
import {
  useCallback,
  useEffect,
  useState,
  type ChangeEvent,
} from "react";
import { EmptyState } from "../components/common/EmptyState";
import { LoadingState } from "../components/common/LoadingState";
import { devicesService } from "../features/devices/devices.service";
import type { Device } from "../features/devices/devices.types";
import { syncLogsService } from "../features/syncLogs/syncLogs.service";
import type {
  SyncLog,
  SyncResult,
} from "../features/syncLogs/syncLogs.types";
import { formatDateTime } from "../utils/fileFormatters";
import {
  getSyncResultClassName,
  getSyncResultLabel,
} from "../utils/syncLogFormatters";

interface SyncLogPageData {
  logs: SyncLog[];
  devices: Device[];
}

function getErrorMessage(error: unknown): string {
  if (!axios.isAxiosError(error)) {
    return "Ocurrió un error inesperado.";
  }

  if (error.response?.status === 400) {
    return "Los filtros seleccionados no son válidos.";
  }

  if (error.response?.status === 401) {
    return "La sesión expiró. Inicia sesión nuevamente.";
  }

  return "No fue posible cargar las sincronizaciones.";
}

async function fetchSyncLogData(
  deviceId: string,
  result: SyncResult | "",
): Promise<SyncLogPageData> {
  const [logs, devices] = await Promise.all([
    syncLogsService.getAll({
      deviceId,
      result,
      limit: 30,
    }),
    devicesService.getAll("all"),
  ]);

  return { logs, devices };
}

export function SyncLogsPage() {
  const [logs, setLogs] = useState<SyncLog[]>([]);
  const [devices, setDevices] = useState<Device[]>([]);
  const [deviceId, setDeviceId] = useState("");
  const [result, setResult] = useState<SyncResult | "">("");
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");

  const loadData = useCallback(async () => {
    setIsLoading(true);
    setErrorMessage("");

    try {
      const data = await fetchSyncLogData(deviceId, result);
      setLogs(data.logs);
      setDevices(data.devices);
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsLoading(false);
    }
  }, [deviceId, result]);

  useEffect(() => {
    let isCancelled = false;

    void fetchSyncLogData(deviceId, result)
      .then((data) => {
        if (!isCancelled) {
          setLogs(data.logs);
          setDevices(data.devices);
        }
      })
      .catch((error: unknown) => {
        if (!isCancelled) {
          setErrorMessage(getErrorMessage(error));
        }
      })
      .finally(() => {
        if (!isCancelled) {
          setIsLoading(false);
        }
      });

    return () => {
      isCancelled = true;
    };
  }, [deviceId, result]);

  const handleDeviceChange = (
    event: ChangeEvent<HTMLSelectElement>,
  ) => {
    setDeviceId(event.target.value);
    setIsLoading(true);
    setErrorMessage("");
  };

  const handleResultChange = (
    event: ChangeEvent<HTMLSelectElement>,
  ) => {
    setResult(event.target.value as SyncResult | "");
    setIsLoading(true);
    setErrorMessage("");
  };

  return (
    <section>
      <div className="page-heading">
        <div>
          <p className="page-heading__eyebrow">Monitoreo</p>
          <h2>Sincronizaciones</h2>
          <p>
            Consulta el resultado de las sincronizaciones realizadas
            por los dispositivos.
          </p>
        </div>

        <div className="page-heading__actions">
          <button
            type="button"
            className="button button--secondary"
            onClick={() => void loadData()}
          >
            <RefreshCw size={18} aria-hidden="true" />
            Actualizar
          </button>
        </div>
      </div>

      {errorMessage && (
        <div className="alert alert--error" role="alert">
          {errorMessage}
        </div>
      )}

      <section className="panel">
        <div className="sync-log-filters">
          <div className="form-field">
            <label htmlFor="sync-device-filter">Dispositivo</label>
            <select
              id="sync-device-filter"
              value={deviceId}
              onChange={handleDeviceChange}
            >
              <option value="">Todos los dispositivos</option>
              {devices.map((device) => (
                <option key={device.id} value={device.id}>
                  {device.name}
                </option>
              ))}
            </select>
          </div>

          <div className="form-field">
            <label htmlFor="sync-result-filter">Resultado</label>
            <select
              id="sync-result-filter"
              value={result}
              onChange={handleResultChange}
            >
              <option value="">Todos los resultados</option>
              <option value="Success">Correcta</option>
              <option value="Failed">Fallida</option>
              <option value="NoChanges">Sin cambios</option>
            </select>
          </div>
        </div>
      </section>

      <section className="panel">
        {isLoading ? (
          <LoadingState message="Cargando sincronizaciones..." />
        ) : logs.length === 0 ? (
          <EmptyState
            title="No hay sincronizaciones"
            description="No se encontraron registros para los filtros seleccionados."
          />
        ) : (
          <div className="table-wrapper">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Dispositivo</th>
                  <th>Resultado</th>
                  <th>Versión</th>
                  <th>Inicio</th>
                  <th>Fin</th>
                  <th>Descargados</th>
                  <th>Eliminados</th>
                  <th>Mensaje</th>
                </tr>
              </thead>
              <tbody>
                {logs.map((log) => (
                  <tr key={log.id}>
                    <td>
                      <strong>{log.deviceName}</strong>
                      <small>
                        {log.playlistName ?? "Sin playlist"}
                      </small>
                    </td>
                    <td>
                      <span
                        className={getSyncResultClassName(log.result)}
                      >
                        {getSyncResultLabel(log.result)}
                      </span>
                    </td>
                    <td>{log.syncedVersion}</td>
                    <td>{formatDateTime(log.startedAt)}</td>
                    <td>
                      {log.finishedAt
                        ? formatDateTime(log.finishedAt)
                        : "En proceso"}
                    </td>
                    <td>{log.downloadedFilesCount}</td>
                    <td>{log.deletedFilesCount}</td>
                    <td>{log.message ?? "Sin mensaje"}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </section>
  );
}
