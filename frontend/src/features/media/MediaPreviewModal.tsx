import { Download, X } from "lucide-react";
import { useEffect, useState } from "react";
import { mediaService } from "./media.service";
import type { MediaItem } from "./media.types";

interface MediaPreviewModalProps {
  item: MediaItem;
  onClose: () => void;
  onDownload: (item: MediaItem) => Promise<void>;
}

export function MediaPreviewModal({
  item,
  onClose,
  onDownload,
}: MediaPreviewModalProps) {
  const [source, setSource] = useState<string | null>(null);
  const [hasError, setHasError] = useState(false);

  useEffect(() => {
    let cancelled = false;
    let objectUrl: string | null = null;
    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        onClose();
      }
    };

    document.addEventListener("keydown", handleKeyDown);

    void mediaService
      .getFile(item.id)
      .then((file) => {
        objectUrl = URL.createObjectURL(file);
        if (cancelled) {
          URL.revokeObjectURL(objectUrl);
          objectUrl = null;
          return;
        }

        setSource(objectUrl);
      })
      .catch(() => {
        if (!cancelled) {
          setHasError(true);
        }
      });

    return () => {
      cancelled = true;
      document.body.style.overflow = previousOverflow;
      document.removeEventListener("keydown", handleKeyDown);
      if (objectUrl) {
        URL.revokeObjectURL(objectUrl);
      }
    };
  }, [item.id, onClose]);

  return (
    <div
      className="media-modal-backdrop media-preview-backdrop"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) {
          onClose();
        }
      }}
    >
      <section
        className="media-preview-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="media-preview-title"
      >
        <header className="media-preview-modal__header">
          <h3 id="media-preview-title" title={item.originalFileName}>
            {item.originalFileName}
          </h3>

          <div>
            <button
              type="button"
              className="icon-button"
              title="Descargar"
              aria-label={`Descargar ${item.originalFileName}`}
              onClick={() => void onDownload(item)}
            >
              <Download size={19} aria-hidden="true" />
            </button>
            <button
              type="button"
              className="icon-button"
              aria-label="Cerrar vista previa"
              onClick={onClose}
            >
              <X size={21} aria-hidden="true" />
            </button>
          </div>
        </header>

        <div className="media-preview-modal__content">
          {!source && !hasError && <span>Cargando archivo...</span>}
          {hasError && <span>No fue posible cargar la vista previa.</span>}
          {source && item.mediaType === "Image" && (
            <img src={source} alt={item.originalFileName} />
          )}
          {source && item.mediaType === "Video" && (
            <video src={source} controls autoPlay playsInline>
              Tu navegador no puede reproducir este video.
            </video>
          )}
        </div>
      </section>
    </div>
  );
}
