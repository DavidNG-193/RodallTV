import { useState, type FormEvent } from "react";
import type {
  CreateMediaFolderRequest,
  MediaFolder,
  UpdateMediaFolderRequest,
} from "./mediaFolders.types";

interface MediaFolderFormProps {
  folder?: MediaFolder | null;
  isSubmitting: boolean;
  onSubmit: (
    request:
      | CreateMediaFolderRequest
      | UpdateMediaFolderRequest,
  ) => Promise<void>;
  onCancel: () => void;
}

export function MediaFolderForm({
  folder,
  isSubmitting,
  onSubmit,
  onCancel,
}: MediaFolderFormProps) {
  const [name, setName] = useState(folder?.name ?? "");
  const [description, setDescription] = useState(
    folder?.description ?? "",
  );

  const handleSubmit = async (
    event: FormEvent<HTMLFormElement>,
  ) => {
    event.preventDefault();

    await onSubmit({
      name: name.trim(),
      description: description.trim() || null,
    });
  };

  return (
    <form
      className="media-folder-form"
      onSubmit={handleSubmit}
    >
      <div className="form-field">
        <label htmlFor="folder-name">Nombre</label>

        <input
          id="folder-name"
          type="text"
          value={name}
          onChange={(event) => setName(event.target.value)}
          maxLength={100}
          placeholder="Ejemplo: Comunicados internos"
          required
        />
      </div>

      <div className="form-field">
        <label htmlFor="folder-description">
          Descripción
        </label>

        <textarea
          id="folder-description"
          value={description}
          onChange={(event) =>
            setDescription(event.target.value)
          }
          maxLength={500}
          placeholder="Descripción opcional"
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
            : folder
              ? "Guardar cambios"
              : "Crear carpeta"}
        </button>
      </div>
    </form>
  );
}
