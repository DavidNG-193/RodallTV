import { useState, type FormEvent } from "react";
import type { MediaItem } from "../media/media.types";
import type {
  CreatePlaylistItemRequest,
  PlaylistItem,
  UpdatePlaylistItemRequest,
} from "./playlists.types";

interface PlaylistItemFormProps {
  media: MediaItem[];
  item?: PlaylistItem | null;
  isSubmitting: boolean;
  onSubmit: (
    request: CreatePlaylistItemRequest | UpdatePlaylistItemRequest,
  ) => Promise<void>;
  onCancel: () => void;
}

export function PlaylistItemForm({
  ...props
}: PlaylistItemFormProps) {
  const formKey = props.item
    ? `${props.item.id}-${props.item.customDurationSeconds ?? "default"}`
    : "new-playlist-item";

  return <PlaylistItemFormFields key={formKey} {...props} />;
}

function PlaylistItemFormFields({
  media,
  item,
  isSubmitting,
  onSubmit,
  onCancel,
}: PlaylistItemFormProps) {
  const [mediaId, setMediaId] = useState(item?.mediaId ?? "");
  const [customDurationSeconds, setCustomDurationSeconds] = useState(
    item?.customDurationSeconds?.toString() ?? "",
  );

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    const duration = customDurationSeconds.trim()
      ? Number(customDurationSeconds)
      : null;

    if (item) {
      await onSubmit({ customDurationSeconds: duration });
      return;
    }

    await onSubmit({
      mediaId,
      customDurationSeconds: duration,
    });
  };

  return (
    <form className="playlist-item-form" onSubmit={handleSubmit}>
      {!item && (
        <div className="form-field">
          <label htmlFor="playlist-media">Archivo multimedia</label>
          <select
            id="playlist-media"
            value={mediaId}
            onChange={(event) => setMediaId(event.target.value)}
            required
          >
            <option value="">Selecciona un archivo</option>
            {media.map((mediaItem) => (
              <option key={mediaItem.id} value={mediaItem.id}>
                {mediaItem.originalFileName}
              </option>
            ))}
          </select>
        </div>
      )}

      <div className="form-field">
        <label htmlFor="custom-duration">Duración personalizada</label>
        <input
          id="custom-duration"
          type="number"
          min="1"
          step="1"
          value={customDurationSeconds}
          onChange={(event) => setCustomDurationSeconds(event.target.value)}
          placeholder="Vacío para usar la duración predeterminada"
        />
      </div>

      <div className="form-actions">
        <button
          type="button"
          className="button button--secondary"
          onClick={onCancel}
          disabled={isSubmitting}
        >
          Cancelar
        </button>

        <button
          type="submit"
          className="button button--primary"
          disabled={
            isSubmitting ||
            (!item && !mediaId) ||
            (customDurationSeconds !== "" && Number(customDurationSeconds) <= 0)
          }
        >
          {isSubmitting
            ? "Guardando..."
            : item
              ? "Guardar duración"
              : "Agregar elemento"}
        </button>
      </div>
    </form>
  );
}
