import type { LucideIcon } from "lucide-react";
import { Link } from "react-router-dom";

interface DashboardMetricCardProps {
  title: string;
  value: number;
  description: string;
  icon: LucideIcon;
  linkTo: string;
  variant?: "default" | "success" | "warning" | "danger";
}

export function DashboardMetricCard({
  title,
  value,
  description,
  icon: Icon,
  linkTo,
  variant = "default",
}: DashboardMetricCardProps) {
  return (
    <Link
      to={linkTo}
      className={`dashboard-metric dashboard-metric--${variant}`}
    >
      <div className="dashboard-metric__icon">
        <Icon size={23} aria-hidden="true" />
      </div>

      <div>
        <span className="dashboard-metric__title">
          {title}
        </span>

        <strong className="dashboard-metric__value">
          {value}
        </strong>

        <small>{description}</small>
      </div>
    </Link>
  );
}