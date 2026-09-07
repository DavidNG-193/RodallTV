import { X } from "lucide-react";
import { useEffect } from "react";
import { formatDateTime, formatDuration, formatFileSize } from "../../utils/fileFormatters";
import type { MediaItem } from "./media.types";

interface MediaDetailsModalProps {
  item: MediaItem;
  onClose: () => void;
}

export function MediaDetailsModal({ item, onClose }: MediaDetailsModalProps) {
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        onClose();
      }
    };
    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [onClose]);

  return (
    <div
      className="media-modal-backdrop"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) {
          onClose();
        }
      }}
    >
      <section
        className="media-details-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="media-details-title"
      >
        <header>
          <div>
            <span>Información del archivo</span>
            <h3 id="media-details-title">{item.originalFileName}</h3>
          </div>
          <button type="button" className="icon-button" aria-label="Cerrar detalles" onClick={onClose}>
            <X size={20} aria-hidden="true" />
          </button>
        </header>

        <dl className="media-details-list">
          <div><dt>Tipo</dt><dd>{item.mediaType === "Image" ? "Imagen" : "Video"}</dd></div>
          <div><dt>Formato</dt><dd>{item.mimeType}</dd></div>
          <div><dt>Tamaño</dt><dd>{formatFileSize(item.fileSizeBytes)}</dd></div>
          {item.mediaType === "Video" && (
            <div><dt>Duración</dt><dd>{formatDuration(item.durationSeconds)}</dd></div>
          )}
          <div><dt>Carpeta</dt><dd>{item.mediaFolderName ?? "Raíz"}</dd></div>
          <div><dt>Subido</dt><dd>{formatDateTime(item.uploadedAt)}</dd></div>
          <div><dt>Subido por</dt><dd>{item.uploadedByName || "No disponible"}</dd></div>
        </dl>
      </section>
    </div>
  );
}
