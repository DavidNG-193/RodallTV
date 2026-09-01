import { ArrowLeft, ShieldX } from "lucide-react";
import { Link } from "react-router-dom";
import { getFirstAllowedRoute } from "../features/auth/getFirstAllowedRoute";
import { useAuth } from "../features/auth/useAuth";

export function AccessDeniedPage() {
  const { user } = useAuth();

  return (
    <div className="access-denied-page">
      <section className="access-denied-card">
        <span className="access-denied-card__icon"><ShieldX size={34} aria-hidden="true" /></span>
        <p className="page-eyebrow">Permisos insuficientes</p>
        <h1>Acceso denegado</h1>
        <p>Tu cuenta no tiene permisos para acceder a esta sección.</p>
        <Link className="button button--primary" to={getFirstAllowedRoute(user?.permissions ?? [])}>
          <ArrowLeft size={18} aria-hidden="true" />
          Volver a una sección disponible
        </Link>
      </section>
    </div>
  );
}
