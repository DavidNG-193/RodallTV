import { LogOut } from "lucide-react";
import { useAuth } from "../../features/auth/useAuth";

export function Header() {
  const { user, logout } = useAuth();

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

        <button type="button" className="button button--ghost" onClick={logout}>
          <LogOut size={18} aria-hidden="true" />
          Cerrar sesión
        </button>
      </div>
    </header>
  );
}