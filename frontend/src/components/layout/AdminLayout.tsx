import { Outlet } from "react-router-dom";
import { Header } from "./Header";
import { Sidebar } from "./Sidebar";

export function AdminLayout() {
  return (
    <div className="admin-shell">
      <Sidebar />
      <div className="admin-shell__content">
        <Header />
        <main className="page-content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}