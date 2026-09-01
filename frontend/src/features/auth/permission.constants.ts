export const PERMISSIONS = {
  DASHBOARD_VIEW: "dashboard.view",

  DEVICES_VIEW: "devices.view",
  DEVICES_MANAGE: "devices.manage",

  MEDIA_VIEW: "media.view",
  MEDIA_MANAGE: "media.manage",

  PLAYLISTS_VIEW: "playlists.view",
  PLAYLISTS_MANAGE: "playlists.manage",

  ASSIGNMENTS_VIEW: "assignments.view",
  ASSIGNMENTS_MANAGE: "assignments.manage",

  REFERENCES_VIEW: "references.view",
  REFERENCES_MANAGE: "references.manage",

  SYNC_LOGS_VIEW: "sync_logs.view",

  USERS_MANAGE: "users.manage",
} as const;

export type PermissionCode =
  (typeof PERMISSIONS)[keyof typeof PERMISSIONS];
