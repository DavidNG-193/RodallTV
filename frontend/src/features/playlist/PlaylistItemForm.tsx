import { useState, type FormEvent } from "react";
import type {
  PlaylistItem,
  UpdatePlaylistItemRequest,
} from "./playlists.types";

interface PlaylistItemFormProps {
  item: PlaylistItem;
  isSubmitting: boolean;
  onSubmit: (request: UpdatePlaylistItemRequest) => Promise<void>;
  onCancel: () => void;
}

export function PlaylistItemForm({
  ...props
}: PlaylistItemFormProps) {
  const formKey =
    `${props.item.id}-${props.item.customDurationSeconds ?? "default"}`;

  return <PlaylistItemFormFields key={formKey} {...props} />;
}

function PlaylistItemFormFields({
  item,
  isSubmitting,
  onSubmit,
  onCancel,
}: PlaylistItemFormProps) {
  const [customDurationSeconds, setCustomDurationSeconds] = useState(
    item.customDurationSeconds?.toString() ?? "",
  );

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    const duration = customDurationSeconds.trim()
      ? Number(customDurationSeconds)
      : null;

    await onSubmit({ customDurationSeconds: duration });
  };

  return (
    <form className="playlist-item-form" onSubmit={handleSubmit}>
      <div className="form-field">
        <label htmlFor="custom-duration">
          Duración personalizada (segundos)
        </label>
        <input
          id="custom-duration"
          type="text"
          inputMode="numeric"
          pattern="[0-9]*"
          maxLength={10}
          value={customDurationSeconds}
          onChange={(event) =>
            setCustomDurationSeconds(
              event.target.value.replace(/\D/g, ""),
            )
          }
          placeholder="Ejemplo: 10"
          aria-describedby="custom-duration-help"
        />
        <small id="custom-duration-help">
          Ingresa la duración en segundos o deja el campo vacío para usar
          la duración predeterminada.
        </small>
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
            (customDurationSeconds !== "" &&
              (Number(customDurationSeconds) <= 0 ||
                Number(customDurationSeconds) > 2147483647))
          }
        >
          {isSubmitting ? "Guardando..." : "Guardar duración"}
        </button>
      </div>
    </form>
  );
}
