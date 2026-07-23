import {
  Clapperboard,
  FolderOpen,
  LayoutDashboard,
  ListVideo,
  Monitor,
  ScrollText,
} from "lucide-react";
import type { LucideIcon } from "lucide-react";
import { NavLink } from "react-router-dom";
import { brandAssets } from "../../config/brandAssets";

interface SidebarLink {
  to: string;
  label: string;
  icon: LucideIcon;
  end?: boolean;
}

const links: SidebarLink[] = [
  { to: "/", label: "Resumen", icon: LayoutDashboard, end: true },
  { to: "/devices", label: "Dispositivos", icon: Monitor },
  { to: "/media-folders", label: "Carpetas", icon: FolderOpen },
  { to: "/media", label: "Multimedia", icon: Clapperboard },
  { to: "/playlists", label: "Playlists", icon: ListVideo },
  { to: "/sync-logs", label: "Sincronizaciones", icon: ScrollText },
];

export function Sidebar() {
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
        {links.map(({ to, label, icon: Icon, end }) => (
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
