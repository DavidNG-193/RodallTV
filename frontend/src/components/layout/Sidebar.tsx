import {
  FolderOpen,
  LayoutDashboard,
  Link2,
  ListVideo,
  Monitor,
  ScrollText,
  ClipboardList,
  Users,
} from "lucide-react";
import type { LucideIcon } from "lucide-react";
import { NavLink } from "react-router-dom";
import { brandAssets } from "../../config/brandAssets";
import { PERMISSIONS } from "../../features/auth/permission.constants";
import { useAuth } from "../../features/auth/useAuth";

interface SidebarLink {
  to: string;
  label: string;
  icon: LucideIcon;
  end?: boolean;
  permission: string;
}

const links: SidebarLink[] = [
  { to: "/", label: "Resumen", icon: LayoutDashboard, end: true, permission: PERMISSIONS.DASHBOARD_VIEW },
  { to: "/devices", label: "Dispositivos", icon: Monitor, permission: PERMISSIONS.DEVICES_VIEW },
  { to: "/media", label: "Archivos", icon: FolderOpen, permission: PERMISSIONS.MEDIA_VIEW },
  { to: "/playlists", label: "Playlists", icon: ListVideo, permission: PERMISSIONS.PLAYLISTS_VIEW },
  { to: "/assignments", label: "Asignaciones", icon: Link2, permission: PERMISSIONS.ASSIGNMENTS_VIEW },
  { to: "/references", label: "Referencias", icon: ClipboardList, permission: PERMISSIONS.REFERENCES_VIEW },
  { to: "/sync-logs", label: "Sincronizaciones", icon: ScrollText, permission: PERMISSIONS.SYNC_LOGS_VIEW },
  { to: "/users", label: "Usuarios", icon: Users, permission: PERMISSIONS.USERS_MANAGE },
];

export function Sidebar() {
  const { hasPermission } = useAuth();

  return (
    <aside className="sidebar">
      <div className="sidebar__brand">
        <img
          className="sidebar__brand-logo"
          src={brandAssets.secondaryLogo}
          alt="Rodall"
        />
      </div>

      <nav className="sidebar__nav" aria-label="Navegación principal">
        {links.filter(({ permission }) => hasPermission(permission)).map(({ to, label, icon: Icon, end }) => (
          <NavLink
            key={to}
            to={to}
            end={end}
            className={({ isActive }) =>
              `sidebar__link${isActive ? " sidebar__link--active" : ""}`
            }
          >
            <Icon size={19} aria-hidden="true" />
            <span>{label}</span>
          </NavLink>
        ))}
      </nav>
    </aside>
  );
}
