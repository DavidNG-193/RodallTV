import { PERMISSIONS } from "./permission.constants";

export function getFirstAllowedRoute(
  permissions: string[],
): string {
  if (permissions.includes(PERMISSIONS.DASHBOARD_VIEW))
    return "/";

  if (permissions.includes(PERMISSIONS.DEVICES_VIEW))
    return "/devices";

  if (permissions.includes(PERMISSIONS.MEDIA_VIEW))
    return "/media";

  if (permissions.includes(PERMISSIONS.PLAYLISTS_VIEW))
    return "/playlists";

  if (permissions.includes(PERMISSIONS.ASSIGNMENTS_VIEW))
    return "/assignments";

  if (permissions.includes(PERMISSIONS.REFERENCES_VIEW))
    return "/references";

  if (permissions.includes(PERMISSIONS.SYNC_LOGS_VIEW))
    return "/sync-logs";

  if (permissions.includes(PERMISSIONS.USERS_MANAGE))
    return "/users";

  return "/access-denied";
}
