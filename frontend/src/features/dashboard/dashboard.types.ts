import type { Device } from "../devices/devices.types";
import type { MediaItem } from "../media/media.types";
import type { Playlist } from "../playlist/playlists.types";
import type { PlaylistAssignment } from "../assignments/assignments.types";
import type { SyncLog } from "../syncLogs/syncLogs.types";

export interface DashboardData {
  devices: Device[];
  media: MediaItem[];
  playlists: Playlist[];
  assignments: PlaylistAssignment[];
  syncLogs: SyncLog[];
}

export interface DashboardMetrics {
  activeDevices: number;
  onlineDevices: number;
  errorDevices: number;
  activeMedia: number;
  activePlaylists: number;
  activeAssignments: number;
  failedSyncs: number;
}

export interface SystemHealth {
  level: "healthy" | "warning" | "critical";
  title: string;
  description: string;
}