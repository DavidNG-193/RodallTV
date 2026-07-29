import {
  CircleAlert,
  Images,
  Link2,
  ListVideo,
  Monitor,
  RefreshCw,
  Wifi,
} from "lucide-react";
import {
  useCallback,
  useEffect,
  useMemo,
  useState,
} from "react";
import { LoadingState } from "../components/common/LoadingState";
import { DashboardAlert } from "../features/dashboard/DashboardAlert";
import { DashboardMetricCard } from "../features/dashboard/DashboardMetricCard";
import { RecentSyncTable } from "../features/dashboard/RecentSyncTable";
import { SystemStatusPanel } from "../features/dashboard/SystemStatusPanel";
import { dashboardService } from "../features/dashboard/dashboard.service";
import type { DashboardData } from "../features/dashboard/dashboard.types";

export function DashboardPage() {
  const [data, setData] = useState<DashboardData | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");

  const loadDashboard = useCallback(async () => {
    setIsLoading(true);
    setErrorMessage("");

    try {
      setData(await dashboardService.getData());
    } catch {
      setErrorMessage(
        "No fue posible cargar el resumen del sistema.",
      );
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    let isCancelled = false;

    void dashboardService
      .getData()
      .then((dashboardData) => {
        if (!isCancelled) {
          setData(dashboardData);
        }
      })
      .catch(() => {
        if (!isCancelled) {
          setErrorMessage(
            "No fue posible cargar el resumen del sistema.",
          );
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
  }, []);

  const metrics = useMemo(
    () =>
      data
        ? dashboardService.calculateMetrics(data)
        : null,
    [data],
  );

  const health = useMemo(
    () =>
      metrics
        ? dashboardService.calculateSystemHealth(metrics)
        : null,
    [metrics],
  );

  const recentSyncLogs = useMemo(
    () =>
      data
        ? [...data.syncLogs]
            .sort(
              (first, second) =>
                new Date(second.startedAt).getTime() -
                new Date(first.startedAt).getTime(),
            )
            .slice(0, 5)
        : [],
    [data],
  );

  return (
    <section>
      <div className="page-heading">
        <div>
          <p className="page-heading__eyebrow">Resumen</p>
          <h2>Panel principal</h2>
          <p>
            Consulta el estado general del sistema de RodallTV.
          </p>
        </div>

        <div className="page-heading__actions">
          <button
            type="button"
            className="button button--secondary"
            onClick={() => void loadDashboard()}
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

      {isLoading && !data ? (
        <section className="panel">
          <LoadingState message="Cargando resumen del sistema..." />
        </section>
      ) : data && metrics && health ? (
        <>
          <SystemStatusPanel health={health} />

          {metrics.errorDevices > 0 && (
            <DashboardAlert
              title="Dispositivos con error"
              description={`${metrics.errorDevices} dispositivo(s) requieren atención.`}
              linkTo="/devices"
              linkLabel="Revisar dispositivos"
            />
          )}

          {metrics.failedSyncs > 0 && (
            <DashboardAlert
              title="Sincronizaciones fallidas recientes"
              description={`${metrics.failedSyncs} sincronización(es) fallaron en las últimas 24 horas.`}
              linkTo="/sync-logs"
              linkLabel="Ver historial"
            />
          )}

          <div className="dashboard-metric-grid">
            <DashboardMetricCard
              title="Dispositivos activos"
              value={metrics.activeDevices}
              description="Registrados y habilitados"
              icon={Monitor}
              linkTo="/devices"
            />

            <DashboardMetricCard
              title="Dispositivos en línea"
              value={metrics.onlineDevices}
              description="Conectados actualmente"
              icon={Wifi}
              linkTo="/devices"
              variant="success"
            />

            <DashboardMetricCard
              title="Dispositivos con error"
              value={metrics.errorDevices}
              description="Requieren revisión"
              icon={CircleAlert}
              linkTo="/devices"
              variant={
                metrics.errorDevices > 0
                  ? "danger"
                  : "default"
              }
            />

            <DashboardMetricCard
              title="Contenido multimedia"
              value={metrics.activeMedia}
              description="Archivos activos"
              icon={Images}
              linkTo="/media"
            />

            <DashboardMetricCard
              title="Playlists"
              value={metrics.activePlaylists}
              description="Playlists activas"
              icon={ListVideo}
              linkTo="/playlists"
            />

            <DashboardMetricCard
              title="Asignaciones"
              value={metrics.activeAssignments}
              description="Asignaciones activas"
              icon={Link2}
              linkTo="/assignments"
            />
          </div>

          <RecentSyncTable
            logs={recentSyncLogs}
            devices={data.devices}
          />
        </>
      ) : null}
    </section>
  );
}
