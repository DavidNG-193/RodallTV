import { Navigate, Route, Routes } from "react-router-dom";
import { AdminLayout } from "../components/layout/AdminLayout";
import { DashboardPage } from "../pages/DashboardPage";
import { DevicesPage } from "../pages/DevicesPage";
import { LoginPage } from "../pages/LoginPage";
import { ProtectedRoute } from "./ProtectedRoute";
import { MediaPage } from "../pages/MediaPage";
import { PlaylistDetailPage } from "../pages/PlaylistDetailPage";
import { PlaylistsPage } from "../pages/PlaylistsPage";
import { AssignmentsPage } from "../pages/AssignmentsPage";
import { SyncLogsPage } from "../pages/SyncLogsPage";
import { DailyReferencesPage } from "../features/references/DailyReferencesPage";
import { AccessDeniedPage } from "../pages/AccessDeniedPage";
import { ChangePasswordPage } from "../pages/ChangePasswordPage";
import { UsersPage } from "../features/users/UsersPage";
import { PERMISSIONS } from "../features/auth/permission.constants";
import { AuthorizationStatusHandler } from "./AuthorizationStatusHandler";
import { PasswordChangeGuard } from "./PasswordChangeGuard";
import { PermissionRoute } from "./PermissionRoute";

export function AppRouter() {
  return (
    <>
      <AuthorizationStatusHandler />
      <Routes>
        <Route path="/login" element={<LoginPage />} />

        <Route element={<ProtectedRoute><PasswordChangeGuard /></ProtectedRoute>}>
          <Route path="/change-password" element={<ChangePasswordPage />} />

          <Route element={<AdminLayout />}>
            <Route path="/access-denied" element={<AccessDeniedPage />} />

            <Route element={<PermissionRoute permission={PERMISSIONS.DASHBOARD_VIEW} />}>
              <Route path="/" element={<DashboardPage />} />
              <Route path="/dashboard" element={<Navigate to="/" replace />} />
            </Route>
            <Route element={<PermissionRoute permission={PERMISSIONS.DEVICES_VIEW} />}>
              <Route path="/devices" element={<DevicesPage />} />
            </Route>
            <Route element={<PermissionRoute permission={PERMISSIONS.MEDIA_VIEW} />}>
              <Route path="/media-folders" element={<Navigate to="/media" replace />} />
              <Route path="/media" element={<MediaPage />} />
            </Route>
            <Route element={<PermissionRoute permission={PERMISSIONS.PLAYLISTS_VIEW} />}>
              <Route path="/playlists" element={<PlaylistsPage />} />
              <Route path="/playlists/:playlistId" element={<PlaylistDetailPage />} />
            </Route>
            <Route element={<PermissionRoute permission={PERMISSIONS.ASSIGNMENTS_VIEW} />}>
              <Route path="/assignments" element={<AssignmentsPage />} />
            </Route>
            <Route element={<PermissionRoute permission={PERMISSIONS.SYNC_LOGS_VIEW} />}>
              <Route path="/sync-logs" element={<SyncLogsPage />} />
            </Route>
            <Route element={<PermissionRoute permission={PERMISSIONS.REFERENCES_VIEW} />}>
              <Route path="/references" element={<DailyReferencesPage />} />
            </Route>
            <Route element={<PermissionRoute permission={PERMISSIONS.USERS_MANAGE} />}>
              <Route path="/users" element={<UsersPage />} />
            </Route>
          </Route>
        </Route>

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </>
  );
}
