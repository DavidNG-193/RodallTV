import type { ReferenceLookup } from "../types";
import { OperationBadge } from "./OperationBadge";
import { ReferenceStatusBadge } from "./ReferenceStatusBadge";

interface ReferencePreviewProps {
  reference: ReferenceLookup;
  isCreating: boolean;
  onCreate: () => void;
}

export function ReferencePreview({
  reference,
  isCreating,
  onCreate,
}: ReferencePreviewProps) {
  const fields = [
    { label: "Referencia", value: reference.referenceNumber },
    { label: "Cliente", value: reference.client },
    { label: "Aduana", value: reference.customsOffice },
    { label: "Documento / Régimen", value: reference.document },
  ];

  return (
    <div className="reference-preview" aria-live="polite">
      <div className="reference-preview__grid">
        {fields.map((field) => (
          <div className="reference-preview__field" key={field.label}>
            <span>{field.label}</span>
            <strong title={field.value}>{field.value || "Sin dato"}</strong>
          </div>
        ))}

        <div className="reference-preview__field">
          <span>Operación</span>
          <OperationBadge
            code={reference.operationCode}
            label={reference.operation}
          />
        </div>

        <div className="reference-preview__field">
          <span>Estatus</span>
          <ReferenceStatusBadge
            code={reference.statusCode}
            status={reference.status}
          />
        </div>
      </div>

      <button
        type="button"
        className="button references-button--dark"
        disabled={isCreating}
        onClick={onCreate}
      >
        {isCreating ? "Agregando..." : "Agregar a pantallas"}
      </button>
    </div>
  );
}
