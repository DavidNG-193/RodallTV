import { Navigate, Route, Routes } from "react-router-dom";
import { AdminLayout } from "../components/layout/AdminLayout";
import { DashboardPage } from "../pages/DashboardPage";
import { DevicesPage } from "../pages/DevicesPage";
import { LoginPage } from "../pages/LoginPage";
import { ProtectedRoute } from "./ProtectedRoute";
import { MediaFoldersPage } from "../pages/MediaFoldersPage";
import { MediaPage } from "../pages/MediaPage";

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
        <Route path="/media-folders" element={<MediaFoldersPage />} />
        <Route path="/media" element={<MediaPage />} />
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}