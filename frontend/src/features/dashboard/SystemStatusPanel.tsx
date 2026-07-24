import {
  AlertTriangle,
  CheckCircle2,
  CircleAlert,
} from "lucide-react";
import type { SystemHealth } from "./dashboard.types";

interface SystemStatusPanelProps {
  health: SystemHealth;
}

export function SystemStatusPanel({
  health,
}: SystemStatusPanelProps) {
  const Icon =
    health.level === "healthy"
      ? CheckCircle2
      : health.level === "warning"
        ? AlertTriangle
        : CircleAlert;

  return (
    <section
      className={`system-health system-health--${health.level}`}
    >
      <div className="system-health__icon">
        <Icon size={28} aria-hidden="true" />
      </div>

      <div>
        <h3>{health.title}</h3>
        <p>{health.description}</p>
      </div>
    </section>
  );
}
