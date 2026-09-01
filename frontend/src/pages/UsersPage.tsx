import { ShieldCheck, Users } from "lucide-react";

export function UsersPage() {
  return (
    <section className="page-section">
      <div className="page-heading">
        <div>
          <p className="page-eyebrow">Administración</p>
          <h2>Usuarios</h2>
          <p>La cuenta tiene permiso para administrar usuarios.</p>
        </div>
      </div>

      <div className="users-placeholder panel">
        <span className="users-placeholder__icon"><Users size={30} aria-hidden="true" /></span>
        <div>
          <h3>Integración de permisos lista</h3>
          <p>La interfaz de administración de usuarios se incorporará en el siguiente submódulo.</p>
        </div>
        <ShieldCheck size={24} aria-label="Acceso autorizado" />
      </div>
    </section>
  );
}
