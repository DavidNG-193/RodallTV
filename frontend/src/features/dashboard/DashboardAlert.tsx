import {
  ArrowRight,
  TriangleAlert,
} from "lucide-react";
import { Link } from "react-router-dom";

interface DashboardAlertProps {
  title: string;
  description: string;
  linkTo: string;
  linkLabel: string;
}

export function DashboardAlert({
  title,
  description,
  linkTo,
  linkLabel,
}: DashboardAlertProps) {
  return (
    <article className="dashboard-alert">
      <TriangleAlert size={24} aria-hidden="true" />

      <div>
        <strong>{title}</strong>
        <p>{description}</p>
      </div>

      <Link
        to={linkTo}
        className="dashboard-alert__link"
      >
        {linkLabel}
        <ArrowRight size={17} aria-hidden="true" />
      </Link>
    </article>
  );
}