import { assignmentsService } from "../assignments/assignments.service";
import { devicesService } from "../devices/devices.service";
import { mediaService } from "../media/media.service";
import { playlistsService } from "../playlist/playlists.service";
import { syncLogsService } from "../syncLogs/syncLogs.service";
import type {
  DashboardData,
  DashboardMetrics,
  SystemHealth,
} from "./dashboard.types";

export const dashboardService = {
  async getData(): Promise<DashboardData> {
    const [
      devices,
      media,
      playlists,
      assignments,
      syncLogs,
    ] = await Promise.all([
      devicesService.getAll("all"),
      mediaService.getAll(),
      playlistsService.getAll(),
      assignmentsService.getAll(false),
      syncLogsService.getAll(),
    ]);

    return {
      devices,
      media,
      playlists,
      assignments,
      syncLogs,
    };
  },

  calculateMetrics(
    data: DashboardData,
  ): DashboardMetrics {
    const last24Hours = new Date(
      Date.now() - 24 * 60 * 60 * 1000,
    );

    return {
      activeDevices:
        data.devices.filter(
          (device) => device.isActive,
        ).length,

      onlineDevices:
        data.devices.filter(
          (device) =>
            device.isActive &&
            device.status === "Online",
        ).length,

      errorDevices:
        data.devices.filter(
          (device) =>
            device.isActive &&
            device.status === "Error",
        ).length,

      activeMedia:
        data.media.filter(
          (item) => item.isActive,
        ).length,

      activePlaylists:
        data.playlists.filter(
          (playlist) => playlist.isActive,
        ).length,

      activeAssignments:
        data.assignments.filter(
          (assignment) => assignment.isActive,
        ).length,

      failedSyncs:
        data.syncLogs.filter(
          (log) =>
            log.result === "Failed" &&
            new Date(log.startedAt) >= last24Hours,
        ).length,
    };
  },

  calculateSystemHealth(
    metrics: DashboardMetrics,
  ): SystemHealth {
    if (
      metrics.errorDevices > 0 ||
      metrics.failedSyncs >= 3
    ) {
      return {
        level: "critical",
        title: "El sistema necesita atención",
        description:
          "Existen dispositivos con error o varias sincronizaciones fallidas.",
      };
    }

    if (
      metrics.activeDevices > 0 &&
      metrics.onlineDevices < metrics.activeDevices
    ) {
      return {
        level: "warning",
        title: "Algunos dispositivos están desconectados",
        description:
          "Revisa la conexión de los dispositivos que no aparecen en línea.",
      };
    }

    return {
      level: "healthy",
      title: "El sistema funciona correctamente",
      description:
        "No se detectaron problemas importantes.",
    };
  },
};