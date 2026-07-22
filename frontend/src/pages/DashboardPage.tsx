import { useAuth } from "../features/auth/useAuth";

export function DashboardPage() {
  const { user, logout } = useAuth();

  return (
    <main>
      <h1>Panel administrativo</h1>

      <p>
        Bienvenido, {user?.email}
      </p>

      <p>Rol: {user?.role}</p>

      <button type="button" onClick={logout}>
        Cerrar sesión
      </button>
    </main>
  );
}
