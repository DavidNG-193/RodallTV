import { Trash2 } from "lucide-react";
import {
  formatReferenceDate,
  formatReferenceDateTime,
} from "../referenceFormatters";
import type { DailyReference } from "../types";
import { OperationBadge } from "./OperationBadge";
import { ReferenceStatusBadge } from "./ReferenceStatusBadge";

interface DailyReferencesTableProps {
  references: DailyReference[];
  deletingId: string | null;
  onDelete: (reference: DailyReference) => void;
}

export function DailyReferencesTable({
  references,
  deletingId,
  onDelete,
}: DailyReferencesTableProps) {
  return (
    <div className="table-wrapper references-table-wrapper">
      <table className="data-table references-table">
        <thead>
          <tr>
            <th>Ref.</th>
            <th>Cliente</th>
            <th>Operación</th>
            <th>Aduana / Régimen</th>
            <th>Estatus</th>
            <th>Actualizado</th>
            <th>Acción</th>
          </tr>
        </thead>
        <tbody>
          {references.map((reference) => (
            <tr key={reference.id}>
              <td>
                <strong className="reference-number" title={reference.referenceNumber}>
                  {reference.referenceNumber}
                </strong>
                <small>{formatReferenceDate(reference.referenceDate)}</small>
              </td>
              <td>
                <span className="references-table__truncate" title={reference.client}>
                  {reference.client}
                </span>
              </td>
              <td>
                <OperationBadge
                  code={reference.operationCode}
                  label={reference.operation}
                />
              </td>
              <td>
                <span
                  className="references-table__truncate"
                  title={`${reference.customsOffice} · ${reference.document}`}
                >
                  {reference.customsOffice || "Sin aduana"}
                  {reference.document ? ` · ${reference.document}` : ""}
                </span>
              </td>
              <td>
                <ReferenceStatusBadge
                  code={reference.statusCode}
                  status={reference.status}
                />
              </td>
              <td>{formatReferenceDateTime(reference.lastExternalUpdateAt)}</td>
              <td>
                <button
                  type="button"
                  className="icon-button icon-button--danger"
                  aria-label={`Eliminar ${reference.referenceNumber}`}
                  title="Eliminar referencia"
                  disabled={deletingId === reference.id}
                  onClick={() => onDelete(reference)}
                >
                  <Trash2 size={17} aria-hidden="true" />
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
