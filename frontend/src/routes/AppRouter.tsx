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

export function AppRouter() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route
        element={
          <ProtectedRoute>
            <AdminLayout />
          </ProtectedRoute>
        }
      >
        <Route path="/" element={<DashboardPage />} />
        <Route path="/devices" element={<DevicesPage />} />
        <Route
          path="/media-folders"
          element={<Navigate to="/media" replace />}
        />
        <Route path="/media" element={<MediaPage />} />
        <Route path="/playlists" element={<PlaylistsPage />} />
        <Route path="/playlists/:playlistId" element={<PlaylistDetailPage />}
        />
        <Route path="/assignments" element={<AssignmentsPage />} />
        <Route path="/sync-logs" element={<SyncLogsPage />} />
        <Route path="/references" element={<DailyReferencesPage />} />
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
