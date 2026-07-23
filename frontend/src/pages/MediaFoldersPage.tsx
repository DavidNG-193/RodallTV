import axios from "axios";
import {
  Folder,
  Pencil,
  Plus,
  RefreshCw,
  Trash2,
} from "lucide-react";
import {
  useCallback,
  useEffect,
  useState,
} from "react";
import { EmptyState } from "../components/common/EmptyState";
import { LoadingState } from "../components/common/LoadingState";
import { MediaFolderForm } from "../features/mediaFolders/MediaFolderForm";
import { mediaFoldersService } from "../features/mediaFolders/mediaFolders.service";
import type {
  CreateMediaFolderRequest,
  MediaFolder,
  UpdateMediaFolderRequest,
} from "../features/mediaFolders/mediaFolders.types";

function formatDate(value: string): string {
  return new Intl.DateTimeFormat("es-MX", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

function getErrorMessage(error: unknown): string {
  if (!axios.isAxiosError(error)) {
    return "Ocurrió un error inesperado.";
  }

  if (error.response?.status === 409) {
    return "Ya existe una carpeta con ese nombre.";
  }

  if (error.response?.status === 401) {
    return "La sesión expiró. Inicia sesión nuevamente.";
  }

  if (error.response?.status === 400) {
    return "Los datos enviados no son válidos.";
  }

  return "No fue posible completar la operación.";
}

export function MediaFoldersPage() {
  const [folders, setFolders] = useState<MediaFolder[]>([]);
  const [selectedFolder, setSelectedFolder] =
    useState<MediaFolder | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isFormVisible, setIsFormVisible] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  const loadFolders = useCallback(async () => {
    setIsLoading(true);
    setErrorMessage("");

    try {
      const data = await mediaFoldersService.getAll();
      setFolders(data);
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    let isCancelled = false;

    void mediaFoldersService
      .getAll()
      .then((data) => {
        if (!isCancelled) setFolders(data);
      })
      .catch((error: unknown) => {
        if (!isCancelled) {
          setErrorMessage(getErrorMessage(error));
        }
      })
      .finally(() => {
        if (!isCancelled) setIsLoading(false);
      });

    return () => {
      isCancelled = true;
    };
  }, []);

  const refreshFolders = () => {
    void loadFolders();
  };

  const openCreateForm = () => {
    setSelectedFolder(null);
    setErrorMessage("");
    setIsFormVisible(true);
  };

  const openEditForm = (folder: MediaFolder) => {
    setSelectedFolder(folder);
    setErrorMessage("");
    setIsFormVisible(true);
  };

  const closeForm = () => {
    setSelectedFolder(null);
    setIsFormVisible(false);
  };

  const handleSubmit = async (
    request:
      | CreateMediaFolderRequest
      | UpdateMediaFolderRequest,
  ) => {
    setIsSubmitting(true);
    setErrorMessage("");

    try {
      if (selectedFolder) {
        await mediaFoldersService.update(
          selectedFolder.id,
          request as UpdateMediaFolderRequest,
        );
      } else {
        await mediaFoldersService.create(
          request as CreateMediaFolderRequest,
        );
      }

      await loadFolders();
      closeForm();
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleRemove = async (folder: MediaFolder) => {
    const confirmed = window.confirm(
      `¿Deseas eliminar la carpeta "${folder.name}"?`,
    );

    if (!confirmed) {
      return;
    }

    setErrorMessage("");

    try {
      await mediaFoldersService.remove(folder.id);
      await loadFolders();
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    }
  };

  return (
    <section>
      <div className="page-heading">
        <div>
          <p className="page-heading__eyebrow">
            Biblioteca multimedia
          </p>

          <h2>Carpetas</h2>

          <p>
            Organiza imágenes y videos en categorías.
          </p>
        </div>

        <div className="page-heading__actions">
          <button
            type="button"
            className="button button--secondary"
            onClick={refreshFolders}
          >
            <RefreshCw size={18} aria-hidden="true" />
            Actualizar
          </button>

          <button
            type="button"
            className="button button--primary"
            onClick={openCreateForm}
          >
            <Plus size={18} aria-hidden="true" />
            Nueva carpeta
          </button>
        </div>
      </div>

      {errorMessage && (
        <div
          className="alert alert--error"
          role="alert"
        >
          {errorMessage}
        </div>
      )}

      {isFormVisible && (
        <section className="panel">
          <div className="panel__heading">
            <h3>
              {selectedFolder
                ? "Editar carpeta"
                : "Crear carpeta"}
            </h3>

            <p>
              Define un nombre y una descripción opcional.
            </p>
          </div>

          <MediaFolderForm
            key={selectedFolder?.id ?? "new-folder"}
            folder={selectedFolder}
            isSubmitting={isSubmitting}
            onSubmit={handleSubmit}
            onCancel={closeForm}
          />
        </section>
      )}

      <section className="panel">
        {isLoading ? (
          <LoadingState message="Cargando carpetas..." />
        ) : folders.length === 0 ? (
          <EmptyState
            title="No hay carpetas"
            description="Crea una carpeta para organizar el contenido."
          />
        ) : (
          <div className="folder-grid">
            {folders.map((folder) => (
              <article
                className="folder-card"
                key={folder.id}
              >
                <div className="folder-card__icon">
                  <Folder size={28} aria-hidden="true" />
                </div>

                <div className="folder-card__content">
                  <h3>{folder.name}</h3>

                  <p>
                    {folder.description ?? "Sin descripción"}
                  </p>

                  <small>
                    Creada el {formatDate(folder.createdAt)}
                  </small>
                </div>

                <div className="folder-card__actions">
                  <button
                    type="button"
                    className="icon-button"
                    title="Editar carpeta"
                    onClick={() => openEditForm(folder)}
                  >
                    <Pencil size={17} aria-hidden="true" />
                  </button>

                  <button
                    type="button"
                    className="icon-button icon-button--danger"
                    title="Eliminar carpeta"
                    onClick={() => void handleRemove(folder)}
                  >
                    <Trash2 size={17} aria-hidden="true" />
                  </button>
                </div>
              </article>
            ))}
          </div>
        )}
      </section>
    </section>
  );
}
