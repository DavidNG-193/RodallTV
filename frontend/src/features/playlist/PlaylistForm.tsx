import { useState, type FormEvent } from "react";
import type {
  CreatePlaylistRequest,
  Playlist,
  UpdatePlaylistRequest,
} from "./playlists.types";

interface PlaylistFormProps {
  playlist?: Playlist | null;
  isSubmitting: boolean;
  onSubmit: (
    request: CreatePlaylistRequest | UpdatePlaylistRequest,
  ) => Promise<void>;
  onCancel: () => void;
}

export function PlaylistForm({
  ...props
}: PlaylistFormProps) {
  const formKey = props.playlist
    ? `${props.playlist.id}-${props.playlist.updatedAt}`
    : "new-playlist";

  return <PlaylistFormFields key={formKey} {...props} />;
}

function PlaylistFormFields({
  playlist,
  isSubmitting,
  onSubmit,
  onCancel,
}: PlaylistFormProps) {
  const [name, setName] = useState(playlist?.name ?? "");
  const [description, setDescription] = useState(
    playlist?.description ?? "",
  );

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    await onSubmit({
      name: name.trim(),
      description: description.trim() || null,
    });
  };

  return (
    <form className="playlist-form" onSubmit={handleSubmit}>
      <div className="form-field">
        <label htmlFor="playlist-name">Nombre</label>
        <input
          id="playlist-name"
          value={name}
          onChange={(event) => setName(event.target.value)}
          maxLength={100}
          required
        />
      </div>

      <div className="form-field">
        <label htmlFor="playlist-description">Descripción</label>
        <textarea
          id="playlist-description"
          value={description}
          onChange={(event) => setDescription(event.target.value)}
          maxLength={500}
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
          disabled={isSubmitting || !name.trim()}
        >
          {isSubmitting
            ? "Guardando..."
            : playlist
              ? "Guardar cambios"
              : "Crear playlist"}
        </button>
      </div>
    </form>
  );
}
