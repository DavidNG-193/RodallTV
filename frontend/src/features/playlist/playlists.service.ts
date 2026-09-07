import { httpClient } from "../../api/httpClient";
import type {
  CreatePlaylistItemRequest,
  CreatePlaylistRequest,
  Playlist,
  PlaylistItem,
  ReorderPlaylistItemsRequest,
  SavePlaylistCompositionRequest,
  SavePlaylistCompositionResponse,
  UpdatePlaylistItemRequest,
  UpdatePlaylistRequest,
} from "./playlists.types";

export const playlistsService = {
  async getAll(
    includeInactive = false,
  ): Promise<Playlist[]> {
    const response = await httpClient.get<Playlist[]>(
      "/api/Playlists",
      {
        params: {
          includeInactive,
        },
      },
    );

    return response.data;
  },

  async getById(id: string): Promise<Playlist> {
    const response = await httpClient.get<Playlist>(`/api/Playlists/${id}`);
    return response.data;
  },

  async create(request: CreatePlaylistRequest): Promise<Playlist> {
    const response = await httpClient.post<Playlist>("/api/Playlists", request);
    return response.data;
  },

  async update(id: string, request: UpdatePlaylistRequest): Promise<Playlist> {
    const response = await httpClient.put<Playlist>(`/api/Playlists/${id}`, request);
    return response.data;
  },

  async deactivate(id: string): Promise<void> {
    await httpClient.delete(`/api/Playlists/${id}`);
  },

  async getItems(playlistId: string): Promise<PlaylistItem[]> {
    const response = await httpClient.get<PlaylistItem[]>(
      `/api/playlists/${playlistId}/items`,
    );
    return response.data;
  },

  async addItem(
    playlistId: string,
    request: CreatePlaylistItemRequest,
  ): Promise<PlaylistItem> {
    const response = await httpClient.post<PlaylistItem>(
      `/api/playlists/${playlistId}/items`,
      request,
    );
    return response.data;
  },

  async updateItem(
    playlistId: string,
    itemId: string,
    request: UpdatePlaylistItemRequest,
  ): Promise<PlaylistItem> {
    const response = await httpClient.put<PlaylistItem>(
      `/api/playlists/${playlistId}/items/${itemId}`,
      request,
    );
    return response.data;
  },

  async reorderItems(
    playlistId: string,
    request: ReorderPlaylistItemsRequest,
  ): Promise<void> {
    await httpClient.put(
      `/api/playlists/${playlistId}/items/reorder`,
      request,
    );
  },

  async saveComposition(
    playlistId: string,
    request: SavePlaylistCompositionRequest,
  ): Promise<SavePlaylistCompositionResponse> {
    const response = await httpClient.put<SavePlaylistCompositionResponse>(
      `/api/playlists/${playlistId}/items/composition`,
      request,
    );
    return response.data;
  },

  async removeItem(playlistId: string, itemId: string): Promise<void> {
    await httpClient.delete(
      `/api/playlists/${playlistId}/items/${itemId}`,
    );
  },
};
