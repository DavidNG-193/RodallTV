import { httpClient } from "../../api/httpClient";
import type {
  CreateMediaFolderRequest,
  MediaFolder,
  UpdateMediaFolderRequest,
} from "./mediaFolders.types";

export const mediaFoldersService = {
  async getAll(): Promise<MediaFolder[]> {
    const response = await httpClient.get<MediaFolder[]>(
      "/api/MediaFolders",
    );

    return response.data;
  },

  async create(
    request: CreateMediaFolderRequest,
  ): Promise<MediaFolder> {
    const response = await httpClient.post<MediaFolder>(
      "/api/MediaFolders",
      request,
    );

    return response.data;
  },

  async update(
    id: string,
    request: UpdateMediaFolderRequest,
  ): Promise<MediaFolder> {
    const response = await httpClient.put<MediaFolder>(
      `/api/MediaFolders/${id}`,
      request,
    );

    return response.data;
  },

  async remove(id: string): Promise<void> {
    await httpClient.delete(`/api/MediaFolders/${id}`);
  },
};