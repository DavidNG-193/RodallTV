import { httpClient } from "../../api/httpClient";
import type {
  DailyReference,
  ReferenceLookup,
  RefreshReferencesResponse,
} from "./types";

export const referencesApi = {
  async getDailyReferences(): Promise<DailyReference[]> {
    const response = await httpClient.get<DailyReference[]>(
      "/api/daily-references",
    );

    return response.data;
  },

  async lookupReference(
    referenceNumber: string,
  ): Promise<ReferenceLookup> {
    const response = await httpClient.get<ReferenceLookup>(
      "/api/daily-references/lookup",
      { params: { referenceNumber } },
    );

    return response.data;
  },

  async createDailyReference(
    referenceNumber: string,
  ): Promise<DailyReference> {
    const response = await httpClient.post<DailyReference>(
      "/api/daily-references",
      { referenceNumber },
    );

    return response.data;
  },

  async deleteDailyReference(id: string): Promise<void> {
    await httpClient.delete(`/api/daily-references/${id}`);
  },

  async deleteAllDailyReferences(): Promise<void> {
    await httpClient.delete("/api/daily-references");
  },

  async refreshDailyReferences(): Promise<RefreshReferencesResponse> {
    const response = await httpClient.post<RefreshReferencesResponse>(
      "/api/daily-references/refresh",
      undefined,
      { timeout: 120_000 },
    );

    return response.data;
  },
};
