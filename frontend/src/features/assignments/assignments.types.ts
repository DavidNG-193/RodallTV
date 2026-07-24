export interface PlaylistAssignment {
  id: string;
  deviceId: string;
  deviceName: string;
  playlistId: string;
  playlistName: string;
  playlistVersion: number;
  assignedByUserId: string;
  assignedByEmail: string;
  assignedAt: string;
  unassignedAt: string | null;
  isActive: boolean;
}

export interface CreatePlaylistAssignmentRequest {
  deviceId: string;
  playlistId: string;
}
