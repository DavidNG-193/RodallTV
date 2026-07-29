import axios from "axios";
import {
  ChevronRight,
  Download,
  FileImage,
  FileVideo,
  Folder,
  Home,
  Pencil,
  Plus,
  RefreshCw,
  Trash2,
  Upload,
} from "lucide-react";
import {
  useCallback,
  useEffect,
  useMemo,
  useState,
} from "react";
import { EmptyState } from "../components/common/EmptyState";
import { LoadingState } from "../components/common/LoadingState";
import { MediaUploadForm } from "../features/media/MediaUploadForm";
import { mediaService } from "../features/media/media.service";
import type {
  MediaItem,
  UploadMediaRequest,
} from "../features/media/media.types";
import { MediaFolderForm } from "../features/mediaFolders/MediaFolderForm";
import { mediaFoldersService } from "../features/mediaFolders/mediaFolders.service";
import type {
  CreateMediaFolderRequest,
  MediaFolder,
  UpdateMediaFolderRequest,
} from "../features/mediaFolders/mediaFolders.types";
import {
  formatDateTime,
  formatDuration,
  formatFileSize,
} from "../utils/fileFormatters";

type MediaFilter = "all" | "image" | "video";
type FormMode = "upload" | "folder" | null;

function getErrorMessage(error: unknown): string {
  if (!axios.isAxiosError(error)) {
    return "Ocurrió un error inesperado.";
  }

  if (error.response?.status === 401) {
    return "La sesión expiró. Inicia sesión nuevamente.";
  }

  if (error.response?.status === 400) {
    return "Los datos enviados no son válidos.";
  }

  if (error.response?.status === 413) {
    return "El archivo supera el tamaño permitido.";
  }

  if (error.response?.status === 409) {
    return "Ya existe una carpeta con ese nombre.";
  }

  return "No fue posible completar la operación.";
}

function isImage(media: MediaItem): boolean {
  return (
    media.mediaType.toLowerCase() === "image" ||
    media.mimeType.startsWith("image/")
  );
}

function MediaImagePreview({ item }: { item: MediaItem }) {
  const [source, setSource] = useState<string | null>(null);

  useEffect(() => {
    let isCancelled = false;
    let objectUrl: string | null = null;

    void mediaService
      .getFile(item.id)
      .then((file) => {
        objectUrl = URL.createObjectURL(file);

        if (isCancelled) {
          URL.revokeObjectURL(objectUrl);
          objectUrl = null;
          return;
        }

        setSource(objectUrl);
      })
      .catch(() => {
        if (!isCancelled) {
          setSource(null);
        }
      });

    return () => {
      isCancelled = true;

      if (objectUrl) {
        URL.revokeObjectURL(objectUrl);
      }
    };
  }, [item.id]);

  if (!source) {
    return (
      <div className="file-manager__placeholder">
        <FileImage size={42} aria-hidden="true" />
        <span>Imagen</span>
      </div>
    );
  }

  return (
    <img
      src={source}
      alt={item.originalFileName}
      loading="lazy"
    />
  );
}

export function MediaPage() {
  const [media, setMedia] = useState<MediaItem[]>([]);
  const [folders, setFolders] = useState<MediaFolder[]>([]);
  const [selectedFolderId, setSelectedFolderId] =
    useState<string | null>(null);
  const [selectedFolderForEdit, setSelectedFolderForEdit] =
    useState<MediaFolder | null>(null);
  const [filter, setFilter] = useState<MediaFilter>("all");
  const [formMode, setFormMode] = useState<FormMode>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  const loadData = useCallback(async () => {
    setIsLoading(true);
    setErrorMessage("");

    try {
      const [mediaData, folderData] = await Promise.all([
        mediaService.getAll(),
        mediaFoldersService.getAll(),
      ]);

      setMedia(mediaData);
      setFolders(folderData);
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    let isCancelled = false;

    void Promise.all([
      mediaService.getAll(),
      mediaFoldersService.getAll(),
    ])
      .then(([mediaData, folderData]) => {
        if (!isCancelled) {
          setMedia(mediaData);
          setFolders(folderData);
        }
      })
      .catch((error: unknown) => {
        if (!isCancelled) {
          setErrorMessage(getErrorMessage(error));
        }
      })
      .finally(() => {
        if (!isCancelled) {
          setIsLoading(false);
        }
      });

    return () => {
      isCancelled = true;
    };
  }, []);

  const selectedFolder = useMemo(
    () =>
      folders.find((folder) => folder.id === selectedFolderId) ??
      null,
    [folders, selectedFolderId],
  );

  const folderItemCounts = useMemo(() => {
    const counts = new Map<string, number>();

    for (const item of media) {
      if (item.mediaFolderId) {
        counts.set(
          item.mediaFolderId,
          (counts.get(item.mediaFolderId) ?? 0) + 1,
        );
      }
    }

    return counts;
  }, [media]);

  const visibleMedia = useMemo(
    () =>
      media.filter((item) => {
        const belongsToCurrentFolder =
          selectedFolderId === null
            ? item.mediaFolderId === null
            : item.mediaFolderId === selectedFolderId;

        if (!belongsToCurrentFolder) {
          return false;
        }

        if (filter === "all") {
          return true;
        }

        return filter === "image"
          ? isImage(item)
          : !isImage(item);
      }),
    [filter, media, selectedFolderId],
  );

  const closeForm = () => {
    setFormMode(null);
    setSelectedFolderForEdit(null);
  };

  const openFolder = (folder: MediaFolder) => {
    setSelectedFolderId(folder.id);
    setFilter("all");
    setErrorMessage("");
    closeForm();
  };

  const goToRoot = () => {
    setSelectedFolderId(null);
    setFilter("all");
    setErrorMessage("");
    closeForm();
  };

  const openCreateFolderForm = () => {
    setSelectedFolderForEdit(null);
    setErrorMessage("");
    setFormMode("folder");
  };

  const openEditFolderForm = (folder: MediaFolder) => {
    setSelectedFolderForEdit(folder);
    setErrorMessage("");
    setFormMode("folder");
  };

  const handleUpload = async (request: UploadMediaRequest) => {
    setIsSubmitting(true);
    setErrorMessage("");

    try {
      await mediaService.upload(request);
      await loadData();
      closeForm();
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleFolderSubmit = async (
    request:
      | CreateMediaFolderRequest
      | UpdateMediaFolderRequest,
  ) => {
    setIsSubmitting(true);
    setErrorMessage("");

    try {
      if (selectedFolderForEdit) {
        await mediaFoldersService.update(
          selectedFolderForEdit.id,
          request as UpdateMediaFolderRequest,
        );
      } else {
        await mediaFoldersService.create(
          request as CreateMediaFolderRequest,
        );
      }

      await loadData();
      closeForm();
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleRemoveFolder = async (folder: MediaFolder) => {
    const itemCount = folderItemCounts.get(folder.id) ?? 0;
    const message =
      itemCount > 0
        ? `¿Deseas eliminar "${folder.name}"? Sus ${itemCount} archivo${
            itemCount === 1 ? "" : "s"
          } se moverán a la raíz.`
        : `¿Deseas eliminar la carpeta "${folder.name}"?`;

    if (!window.confirm(message)) {
      return;
    }

    setErrorMessage("");

    try {
      await mediaFoldersService.remove(folder.id);
      await loadData();
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    }
  };

  const handleDeactivate = async (item: MediaItem) => {
    if (
      !window.confirm(
        `¿Deseas desactivar "${item.originalFileName}"?`,
      )
    ) {
      return;
    }

    setErrorMessage("");

    try {
      await mediaService.deactivate(item.id);
      await loadData();
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    }
  };

  const handleDownload = async (item: MediaItem) => {
    setErrorMessage("");

    try {
      await mediaService.download(item);
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    }
  };

  const showsFolders = selectedFolderId === null;
  const hasItems =
    visibleMedia.length > 0 || (showsFolders && folders.length > 0);

  return (
    <section>
      <div className="page-heading">
        <div>
          <p className="page-heading__eyebrow">
            Biblioteca multimedia
          </p>

          <h2>
            {selectedFolder?.name ?? "Archivos y carpetas"}
          </h2>

          <p>
            {selectedFolder
              ? "Contenido guardado en esta carpeta."
              : "Organiza y administra el contenido multimedia."}
          </p>
        </div>

        <div className="page-heading__actions">
          <div className="media-filter">
            <label htmlFor="media-filter">Mostrar</label>

            <select
              id="media-filter"
              value={filter}
              onChange={(event) =>
                setFilter(event.target.value as MediaFilter)
              }
            >
              <option value="all">Todos</option>
              <option value="image">Imágenes</option>
              <option value="video">Videos</option>
            </select>
          </div>

          <button
            type="button"
            className="button button--secondary"
            onClick={() => void loadData()}
            disabled={isLoading}
          >
            <RefreshCw size={18} aria-hidden="true" />
            Actualizar
          </button>

          {showsFolders && (
            <button
              type="button"
              className="button button--secondary"
              onClick={openCreateFolderForm}
            >
              <Plus size={18} aria-hidden="true" />
              Nueva carpeta
            </button>
          )}

          <button
            type="button"
            className="button button--primary"
            onClick={() => {
              setErrorMessage("");
              setFormMode("upload");
              setSelectedFolderForEdit(null);
            }}
          >
            <Upload size={18} aria-hidden="true" />
            Subir archivo
          </button>
        </div>
      </div>

      {errorMessage && (
        <div className="alert alert--error" role="alert">
          {errorMessage}
        </div>
      )}

      {formMode === "upload" && (
        <section className="panel">
          <div className="panel__heading">
            <h3>Subir contenido multimedia</h3>
            <p>
              Elige el archivo y confirma la carpeta de destino.
            </p>
          </div>

          <MediaUploadForm
            key={selectedFolderId ?? "root-upload"}
            folders={folders}
            initialFolderId={selectedFolderId}
            isSubmitting={isSubmitting}
            onSubmit={handleUpload}
            onCancel={closeForm}
          />
        </section>
      )}

      {formMode === "folder" && (
        <section className="panel">
          <div className="panel__heading">
            <h3>
              {selectedFolderForEdit
                ? "Editar carpeta"
                : "Crear carpeta"}
            </h3>
            <p>
              Define un nombre y una descripción opcional.
            </p>
          </div>

          <MediaFolderForm
            key={selectedFolderForEdit?.id ?? "new-folder"}
            folder={selectedFolderForEdit}
            isSubmitting={isSubmitting}
            onSubmit={handleFolderSubmit}
            onCancel={closeForm}
          />
        </section>
      )}

      <nav
        className="file-manager__breadcrumbs"
        aria-label="Ruta actual"
      >
        <button
          type="button"
          onClick={goToRoot}
          className={
            selectedFolder
              ? "file-manager__breadcrumb"
              : "file-manager__breadcrumb file-manager__breadcrumb--current"
          }
          aria-current={selectedFolder ? undefined : "page"}
        >
          <Home size={16} aria-hidden="true" />
          Raíz
        </button>

        {selectedFolder && (
          <>
            <ChevronRight size={16} aria-hidden="true" />
            <span
              className="file-manager__breadcrumb file-manager__breadcrumb--current"
              aria-current="page"
            >
              <Folder size={16} aria-hidden="true" />
              {selectedFolder.name}
            </span>
          </>
        )}
      </nav>

      <section className="panel">
        {isLoading ? (
          <LoadingState message="Cargando contenido..." />
        ) : !hasItems ? (
          <EmptyState
            title={
              selectedFolder
                ? "Esta carpeta está vacía"
                : "No hay contenido"
            }
            description={
              selectedFolder
                ? "Sube un archivo para agregarlo a esta carpeta."
                : "Crea una carpeta o sube un archivo para comenzar."
            }
          />
        ) : (
          <div className="file-manager__grid">
            {showsFolders &&
              folders.map((folder) => {
                const count = folderItemCounts.get(folder.id) ?? 0;

                return (
                  <article
                    className="file-manager-item file-manager-folder"
                    key={folder.id}
                  >
                    <button
                      type="button"
                      className="file-manager-folder__open"
                      onClick={() => openFolder(folder)}
                      aria-label={`Abrir carpeta ${folder.name}`}
                    >
                      <Folder size={58} aria-hidden="true" />
                      <strong title={folder.name}>{folder.name}</strong>
                      <span>
                        {count} archivo{count === 1 ? "" : "s"}
                      </span>
                    </button>

                    <div className="file-manager-folder__actions">
                      <button
                        type="button"
                        className="icon-button"
                        title="Editar carpeta"
                        aria-label={`Editar carpeta ${folder.name}`}
                        onClick={() => openEditFolderForm(folder)}
                      >
                        <Pencil size={17} aria-hidden="true" />
                      </button>

                      <button
                        type="button"
                        className="icon-button icon-button--danger"
                        title="Eliminar carpeta"
                        aria-label={`Eliminar carpeta ${folder.name}`}
                        onClick={() =>
                          void handleRemoveFolder(folder)
                        }
                      >
                        <Trash2 size={17} aria-hidden="true" />
                      </button>
                    </div>
                  </article>
                );
              })}

            {visibleMedia.map((item) => {
              const itemIsImage = isImage(item);

              return (
                <article
                  className="file-manager-item file-manager-media"
                  key={item.id}
                  tabIndex={0}
                >
                  <div className="file-manager-media__preview">
                    {itemIsImage ? (
                      <MediaImagePreview item={item} />
                    ) : (
                      <div className="file-manager__placeholder">
                        <FileVideo size={46} aria-hidden="true" />
                        <span>Video</span>
                      </div>
                    )}
                  </div>

                  <div className="file-manager-media__overlay">
                    <div>
                      <div className="file-manager-media__type">
                        {itemIsImage ? (
                          <FileImage size={15} aria-hidden="true" />
                        ) : (
                          <FileVideo size={15} aria-hidden="true" />
                        )}
                        {itemIsImage ? "Imagen" : "Video"}
                      </div>

                      <h5 title={item.originalFileName}>
                        {item.originalFileName}
                      </h5>

                      <dl>
                        <div>
                          <dt>Tamaño</dt>
                          <dd>{formatFileSize(item.fileSizeBytes)}</dd>
                        </div>

                        {!itemIsImage && (
                          <div>
                            <dt>Duración</dt>
                            <dd>
                              {formatDuration(item.durationSeconds)}
                            </dd>
                          </div>
                        )}

                        <div>
                          <dt>Subido</dt>
                          <dd>{formatDateTime(item.uploadedAt)}</dd>
                        </div>
                      </dl>
                    </div>

                    <div className="file-manager-media__actions">
                      <button
                        type="button"
                        className="icon-button"
                        title="Descargar archivo"
                        aria-label={`Descargar ${item.originalFileName}`}
                        onClick={() => void handleDownload(item)}
                      >
                        <Download size={17} aria-hidden="true" />
                      </button>

                      <button
                        type="button"
                        className="icon-button icon-button--danger"
                        title="Desactivar archivo"
                        aria-label={`Desactivar ${item.originalFileName}`}
                        onClick={() => void handleDeactivate(item)}
                      >
                        <Trash2 size={17} aria-hidden="true" />
                      </button>
                    </div>
                  </div>
                </article>
              );
            })}
          </div>
        )}
      </section>
    </section>
  );
}
