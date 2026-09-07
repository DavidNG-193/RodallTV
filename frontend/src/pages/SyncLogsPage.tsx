import axios from "axios";
import { ChevronLeft, ChevronRight, RefreshCw } from "lucide-react";
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
  PagedSyncLogsResponse,
  SyncLog,
  SyncResult,
} from "../features/syncLogs/syncLogs.types";
import { formatDateTime } from "../utils/fileFormatters";
import {
  getSyncResultClassName,
  getSyncResultLabel,
} from "../utils/syncLogFormatters";

interface SyncLogPageData {
  logs: PagedSyncLogsResponse;
  devices: Device[];
}

const PAGE_SIZE = 20;

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
  page: number,
): Promise<SyncLogPageData> {
  const [logs, devices] = await Promise.all([
    syncLogsService.getPaged({
      deviceId,
      result,
      page,
      pageSize: PAGE_SIZE,
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
  const [page, setPage] = useState(1);
  const [totalItems, setTotalItems] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");

  const loadData = useCallback(async () => {
    setIsLoading(true);
    setErrorMessage("");

    try {
      const data = await fetchSyncLogData(deviceId, result, page);
      setLogs(data.logs.items);
      setDevices(data.devices);
      setTotalItems(data.logs.totalItems);
      setTotalPages(data.logs.totalPages);

      if (data.logs.totalPages > 0 && page > data.logs.totalPages) {
        setPage(data.logs.totalPages);
      }
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsLoading(false);
    }
  }, [deviceId, page, result]);

  useEffect(() => {
    let isCancelled = false;

    void fetchSyncLogData(deviceId, result, page)
      .then((data) => {
        if (!isCancelled) {
          setLogs(data.logs.items);
          setDevices(data.devices);
          setTotalItems(data.logs.totalItems);
          setTotalPages(data.logs.totalPages);

          if (data.logs.totalPages > 0 && page > data.logs.totalPages) {
            setPage(data.logs.totalPages);
          }
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
  }, [deviceId, page, result]);

  const handleDeviceChange = (
    event: ChangeEvent<HTMLSelectElement>,
  ) => {
    setDeviceId(event.target.value);
    setPage(1);
    setIsLoading(true);
    setErrorMessage("");
  };

  const handleResultChange = (
    event: ChangeEvent<HTMLSelectElement>,
  ) => {
    setResult(event.target.value as SyncResult | "");
    setPage(1);
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
          <>
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

            <footer className="media-pagination">
              <span>
                {totalItems} sincronización{totalItems === 1 ? "" : "es"}
                {totalItems === 100 ? " más recientes" : ""}
              </span>
              <div>
                <button
                  type="button"
                  className="icon-button"
                  aria-label="Página anterior"
                  disabled={page <= 1}
                  onClick={() => {
                    setIsLoading(true);
                    setPage((value) => value - 1);
                  }}
                >
                  <ChevronLeft size={18} aria-hidden="true" />
                </button>
                <span>
                  Página {page} de {Math.max(totalPages, 1)}
                </span>
                <button
                  type="button"
                  className="icon-button"
                  aria-label="Página siguiente"
                  disabled={page >= totalPages}
                  onClick={() => {
                    setIsLoading(true);
                    setPage((value) => value + 1);
                  }}
                >
                  <ChevronRight size={18} aria-hidden="true" />
                </button>
              </div>
            </footer>
          </>
        )}
      </section>
    </section>
  );
}
