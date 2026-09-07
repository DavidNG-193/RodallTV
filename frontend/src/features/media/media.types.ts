export type MediaType =
  | "Image"
  | "Video";

export type MediaSort = "recent" | "nameAsc" | "nameDesc";

export interface MediaItem {
  id: string;
  originalFileName: string;
  storedFileName: string;
  fileExtension: string;
  mimeType: string;
  mediaType: MediaType;
  fileSizeBytes: number;
  durationSeconds: number | null;
  hashSha256: string;
  uploadedAt: string;
  uploadedByUserId: string;
  uploadedByName: string;
  mediaFolderId: string | null;
  mediaFolderName: string | null;
  isActive: boolean;
}

export interface UploadMediaRequest {
  file: File;
  mediaFolderId?: string | null;
}

export interface UpdateMediaRequest {
  originalFileName: string;
  mediaFolderId: string | null;
}

export interface MediaQuery {
  page: number;
  pageSize: number;
  search?: string;
  mediaType?: "all" | "image" | "video";
  mediaFolderId?: string | null;
  rootOnly?: boolean;
  sort?: MediaSort;
}

export interface PagedMediaResponse {
  items: MediaItem[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}
