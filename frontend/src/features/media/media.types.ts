export type MediaType =
  | "Image"
  | "Video";

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
