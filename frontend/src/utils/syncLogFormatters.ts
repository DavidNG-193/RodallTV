import type {
  SyncResult,
} from "../features/syncLogs/syncLogs.types";

export function getSyncResultLabel(
  result: SyncResult,
): string {
  const labels: Record<SyncResult, string> = {
    Success: "Correcta",
    Failed: "Fallida",
    NoChanges: "Sin cambios",
  };

  return labels[result];
}

export function getSyncResultClassName(
  result: SyncResult,
): string {
  const classNames: Record<SyncResult, string> = {
    Success: "sync-result sync-result--success",
    Failed: "sync-result sync-result--failed",
    NoChanges: "sync-result sync-result--nochanges",
  };

  return classNames[result];
}
