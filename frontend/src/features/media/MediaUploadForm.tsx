import {
  useEffect,
  useMemo,
  useState,
  type FormEvent,
} from "react";
import type { MediaFolder } from "../mediaFolders/mediaFolders.types";
import type { UploadMediaRequest } from "./media.types";

interface MediaUploadFormProps {
  folders: MediaFolder[];
  isSubmitting: boolean;
  onSubmit: (
    request: UploadMediaRequest,
  ) => Promise<void>;
  onCancel: () => void;
}

const allowedExtensions = [
  ".jpg",
  ".jpeg",
  ".png",
  ".webp",
  ".mp4",
  ".webm",
];

export function MediaUploadForm({
  folders,
  isSubmitting,
  onSubmit,
  onCancel,
}: MediaUploadFormProps) {
  const [file, setFile] =
    useState<File | null>(null);

  const [
    mediaFolderId,
    setMediaFolderId,
  ] = useState("");

  const previewUrl = useMemo(() => {
    if (
      !file ||
      !file.type.startsWith("image/")
    ) {
      return null;
    }

    return URL.createObjectURL(file);
  }, [file]);

  useEffect(
    () => () => {
      if (previewUrl) {
        URL.revokeObjectURL(previewUrl);
      }
    },
    [previewUrl],
  );

  const handleSubmit = async (
    event: FormEvent<HTMLFormElement>,
  ) => {
    event.preventDefault();

    if (!file) {
      return;
    }

    await onSubmit({
      file,
      mediaFolderId:
        mediaFolderId || null,
    });
  };

  return (
    <form
      className="media-upload-form"
      onSubmit={handleSubmit}
    >
      <div className="form-field">
        <label htmlFor="media-file">
          Archivo multimedia
        </label>

        <input
          id="media-file"
          type="file"
          accept={allowedExtensions.join(",")}
          onChange={(event) =>
            setFile(
              event.target.files?.[0] ??
                null,
            )
          }
          required
        />

        <small>
          Formatos permitidos:
          JPG, PNG, WebP, MP4 y WebM.
        </small>
      </div>

      <div className="form-field">
        <label htmlFor="media-folder">
          Carpeta
        </label>

        <select
          id="media-folder"
          value={mediaFolderId}
          onChange={(event) =>
            setMediaFolderId(
              event.target.value,
            )
          }
        >
          <option value="">
            Sin carpeta
          </option>

          {folders.map((folder) => (
            <option
              key={folder.id}
              value={folder.id}
            >
              {folder.name}
            </option>
          ))}
        </select>
      </div>

      {file && (
        <section className="upload-preview">
          <div>
            <strong>
              {file.name}
            </strong>

            <span>
              {file.type ||
                "Tipo desconocido"}
            </span>
          </div>

          {previewUrl && (
            <img
              src={previewUrl}
              alt="Vista previa del archivo"
            />
          )}
        </section>
      )}

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
            isSubmitting || !file
          }
        >
          {isSubmitting
            ? "Subiendo..."
            : "Subir archivo"}
        </button>
      </div>
    </form>
  );
}
