export type DeviceStatus =
  | "Online"
  | "Offline"
  | "Syncing"
  | "Error"
  | "NotSynced";

export type DeviceListFilter = "active" | "inactive" | "all";

export interface Device {
  id: string;
  deviceUuid: string;
  name: string;
  location: string | null;
  status: DeviceStatus;
  ipAddress: string | null;
  currentPlaylistVersion: number;
  agentVersion: string | null;
  lastConnectionAt: string | null;
  lastSyncAt: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface CreateDeviceRequest {
  name: string;
  location?: string | null;
}

export interface CreateDeviceResponse extends Device {
  accessToken: string;
}

export interface UpdateDeviceRequest {
  name: string;
  location?: string | null;
}
