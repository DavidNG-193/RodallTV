import { KeyRound, LogOut } from "lucide-react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../../features/auth/useAuth";

export function Header() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

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
          <strong>{user?.email ?? "Administrador"}</strong>
          <small>{user?.role ?? "Administrator"}</small>
        </div>

        <div className="header__actions">
          <Link className="button button--ghost" to="/change-password">
            <KeyRound size={18} aria-hidden="true" />
            Cambiar contraseña
          </Link>

          <button type="button" className="button button--ghost" onClick={handleLogout}>
            <LogOut size={18} aria-hidden="true" />
            Cerrar sesión
          </button>
        </div>
      </div>
    </header>
  );
}
