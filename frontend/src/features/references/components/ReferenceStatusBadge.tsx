interface ReferenceStatusBadgeProps {
  code: string;
  status: string;
}

export function ReferenceStatusBadge({
  code,
  status,
}: ReferenceStatusBadgeProps) {
  return (
    <span
      className="reference-status-badge"
      title={code ? `Código ${code}` : undefined}
    >
      {status || "Sin estatus"}
    </span>
  );
}
