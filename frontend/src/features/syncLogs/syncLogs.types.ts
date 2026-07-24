export type SyncResult =
  | "Success"
  | "Failed"
  | "NoChanges";

export interface SyncLog {
  id: string;
  deviceId: string;
  deviceName: string;
  playlistId: string | null;
  playlistName: string | null;
  syncedVersion: number;
  startedAt: string;
  finishedAt: string | null;
  result: SyncResult;
  message: string | null;
  downloadedFilesCount: number;
  deletedFilesCount: number;
}
