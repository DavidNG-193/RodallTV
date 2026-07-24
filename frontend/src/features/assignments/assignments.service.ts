import axios from "axios";
import { httpClient } from "../../api/httpClient";
import type {
  CreatePlaylistAssignmentRequest,
  PlaylistAssignment,
} from "./assignments.types";

export const assignmentsService = {
  async getAll(
    includeInactive = true,
  ): Promise<PlaylistAssignment[]> {
    const response =
      await httpClient.get<PlaylistAssignment[]>(
        "/api/playlist-assignments",
        {
          params: { includeInactive },
        },
      );

    return response.data;
  },

  async getActiveByDevice(
    deviceId: string,
  ): Promise<PlaylistAssignment | null> {
    try {
      const response =
        await httpClient.get<PlaylistAssignment>(
          `/api/playlist-assignments/device/${deviceId}/active`,
        );

      return response.data;
    } catch (error: unknown) {
      if (
        axios.isAxiosError(error) &&
        error.response?.status === 404
      ) {
        return null;
      }

      throw error;
    }
  },

  async assign(
    request: CreatePlaylistAssignmentRequest,
  ): Promise<PlaylistAssignment> {
    const response =
      await httpClient.post<PlaylistAssignment>(
        "/api/playlist-assignments",
        request,
      );

    return response.data;
  },

  async unassign(deviceId: string): Promise<void> {
    await httpClient.delete(
      `/api/playlist-assignments/device/${deviceId}/active`,
    );
  },
};
