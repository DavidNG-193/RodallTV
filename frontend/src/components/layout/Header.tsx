import { LogOut } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../features/auth/useAuth";

export function Header() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const displayName = [user?.firstName, user?.lastName]
    .filter(Boolean)
    .join(" ");

  const handleLogout = () => {
    logout();
    navigate("/login", { replace: true });
  };

  return (
    <header className="header">
      <div>
        <p className="header__eyebrow">Panel administrativo</p>
        <h1 className="header__title">Sistema RodallTV</h1>
      </div>

      <div className="header__account">
        <div>
          <strong>{displayName || user?.email || "Administrador"}</strong>
          <small>{user?.role ?? "Administrator"}</small>
        </div>

        <div className="header__actions">
          <button type="button" className="button button--ghost" onClick={handleLogout}>
            <LogOut size={18} aria-hidden="true" />
            Cerrar sesión
          </button>
        </div>
      </div>
    </header>
  );
}
