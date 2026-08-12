import type { DailyReference } from "../types";

interface ReferencesSummaryProps {
  references: DailyReference[];
}

export function ReferencesSummary({ references }: ReferencesSummaryProps) {
  const imports = references.filter(
    (reference) => reference.operationCode.toUpperCase() === "I",
  ).length;
  const exports = references.filter(
    (reference) => reference.operationCode.toUpperCase() === "E",
  ).length;

  return (
    <dl className="references-summary" aria-label="Resumen de referencias">
      <div>
        <dt>Total</dt>
        <dd>{references.length}</dd>
      </div>
      <div>
        <dt>Importaciones</dt>
        <dd>{imports}</dd>
      </div>
      <div>
        <dt>Exportaciones</dt>
        <dd>{exports}</dd>
      </div>
    </dl>
  );
}
