import { httpClient } from "../../api/httpClient";
import type {
  PagedSyncLogsResponse,
  SyncLog,
  SyncResult,
} from "./syncLogs.types";

export interface SyncLogFilters {
  deviceId?: string;
  result?: SyncResult | "";
  limit?: number;
}

export interface SyncLogPageFilters {
  deviceId?: string;
  result?: SyncResult | "";
  page: number;
  pageSize: number;
}

export const syncLogsService = {
  async getAll(
    filters: SyncLogFilters = {},
  ): Promise<SyncLog[]> {
    const response =
      await httpClient.get<SyncLog[]>(
        "/api/sync-logs",
        {
          params: {
            deviceId:
              filters.deviceId || undefined,
            result:
              filters.result || undefined,
            limit: filters.limit,
          },
        },
      );

    return response.data;
  },

  async getPaged(
    filters: SyncLogPageFilters,
  ): Promise<PagedSyncLogsResponse> {
    const response =
      await httpClient.get<PagedSyncLogsResponse>(
        "/api/sync-logs/paged",
        {
          params: {
            deviceId: filters.deviceId || undefined,
            result: filters.result || undefined,
            page: filters.page,
            pageSize: filters.pageSize,
          },
        },
      );

    return response.data;
  },
};
