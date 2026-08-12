interface OperationBadgeProps {
  code: string;
  label: string;
}

export function OperationBadge({ code, label }: OperationBadgeProps) {
  const normalizedCode = code.trim().toUpperCase();
  const tone = normalizedCode === "I"
    ? "import"
    : normalizedCode === "E"
      ? "export"
      : "neutral";

  return (
    <span className={`reference-operation-badge reference-operation-badge--${tone}`}>
      {label || "No especificada"}
    </span>
  );
}
