import { useEffect } from "react";

interface ConfirmationDialogProps {
  title: string;
  message: string;
  confirmLabel: string;
  isBusy: boolean;
  onCancel: () => void;
  onConfirm: () => void;
}

export function ConfirmationDialog({
  title,
  message,
  confirmLabel,
  isBusy,
  onCancel,
  onConfirm,
}: ConfirmationDialogProps) {
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape" && !isBusy) onCancel();
    };

    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [isBusy, onCancel]);

  return (
    <div className="references-dialog-backdrop">
      <section
        className="references-dialog"
        role="alertdialog"
        aria-modal="true"
        aria-labelledby="references-dialog-title"
        aria-describedby="references-dialog-description"
      >
        <span className="references-dialog__eyebrow">Confirmación</span>
        <h3 id="references-dialog-title">{title}</h3>
        <p id="references-dialog-description">{message}</p>

        <div className="references-dialog__actions">
          <button
            type="button"
            className="button button--secondary"
            disabled={isBusy}
            autoFocus
            onClick={onCancel}
          >
            Cancelar
          </button>
          <button
            type="button"
            className="button references-button--danger"
            disabled={isBusy}
            onClick={onConfirm}
          >
            {isBusy ? "Eliminando..." : confirmLabel}
          </button>
        </div>
      </section>
    </div>
  );
}
