import { X } from "lucide-react";
import { useEffect, useState, type FormEvent } from "react";
import type { MediaItem } from "./media.types";

interface MediaRenameModalProps {
  item: MediaItem;
  isSubmitting: boolean;
  onClose: () => void;
  onSubmit: (name: string) => Promise<void>;
}

export function MediaRenameModal({
  item,
  isSubmitting,
  onClose,
  onSubmit,
}: MediaRenameModalProps) {
  const [name, setName] = useState(item.originalFileName);

  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape" && !isSubmitting) {
        onClose();
      }
    };
    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [isSubmitting, onClose]);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    await onSubmit(name.trim());
  };

  return (
    <div className="media-modal-backdrop">
      <section className="media-details-modal" role="dialog" aria-modal="true" aria-labelledby="media-rename-title">
        <header>
          <div>
            <span>Editar archivo</span>
            <h3 id="media-rename-title">Cambiar nombre</h3>
          </div>
          <button type="button" className="icon-button" aria-label="Cerrar" disabled={isSubmitting} onClick={onClose}>
            <X size={20} aria-hidden="true" />
          </button>
        </header>

        <form onSubmit={(event) => void handleSubmit(event)}>
          <div className="form-field">
            <label htmlFor="media-rename-input">Nombre del archivo</label>
            <input
              id="media-rename-input"
              value={name}
              maxLength={255}
              autoFocus
              onChange={(event) => setName(event.target.value)}
              required
            />
            <small>La extensión debe conservarse.</small>
          </div>

          <div className="form-actions">
            <button type="button" className="button button--secondary" disabled={isSubmitting} onClick={onClose}>Cancelar</button>
            <button type="submit" className="button button--primary" disabled={isSubmitting || !name.trim()}>
              {isSubmitting ? "Guardando..." : "Guardar nombre"}
            </button>
          </div>
        </form>
      </section>
    </div>
  );
}
