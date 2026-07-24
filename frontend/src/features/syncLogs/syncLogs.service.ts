import { httpClient } from "../../api/httpClient";
import type {
  SyncLog,
  SyncResult,
} from "./syncLogs.types";

export interface SyncLogFilters {
  deviceId?: string;
  result?: SyncResult | "";
  limit?: number;
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
};
