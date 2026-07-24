import type { MediaType } from "../media/media.types";

export interface Playlist {
  id: string;
  name: string;
  description: string | null;
  version: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  createdByUserId: string;
  createdByName: string;
}

export interface CreatePlaylistRequest {
  name: string;
  description?: string | null;
}

export interface UpdatePlaylistRequest {
  name: string;
  description?: string | null;
}

export interface PlaylistItem {
  id: string;
  playlistId: string;
  mediaId: string;
  originalFileName: string;
  storedFileName: string;
  mediaType: MediaType;
  mimeType: string;
  position: number;
  customDurationSeconds: number | null;
  createdAt: string;
}

export interface CreatePlaylistItemRequest {
  mediaId: string;
  customDurationSeconds?: number | null;
}

export interface UpdatePlaylistItemRequest {
  customDurationSeconds?: number | null;
}

export interface ReorderPlaylistItemsRequest {
  orderedItemIds: string[];
}
