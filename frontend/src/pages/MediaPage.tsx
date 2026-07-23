import axios from "axios";
import {
  Download,
  FileImage,
  FileVideo,
  Plus,
  RefreshCw,
  Trash2,
} from "lucide-react";
import {
  useCallback,
  useEffect,
  useMemo,
  useState,
} from "react";
import { EmptyState } from "../components/common/EmptyState";
import { LoadingState } from "../components/common/LoadingState";
import { mediaFoldersService } from "../features/mediaFolders/mediaFolders.service";
import type { MediaFolder } from "../features/mediaFolders/mediaFolders.types";
import { MediaUploadForm } from "../features/media/MediaUploadForm";
import { mediaService } from "../features/media/media.service";
import type {
  MediaItem,
  UploadMediaRequest,
} from "../features/media/media.types";
import {
  formatDateTime,
  formatDuration,
  formatFileSize,
} from "../utils/fileFormatters";

type MediaFilter =
  | "all"
  | "image"
  | "video";

function getErrorMessage(
  error: unknown,
): string {
  if (!axios.isAxiosError(error)) {
    return "Ocurrió un error inesperado.";
  }

  if (error.response?.status === 401) {
    return "La sesión expiró. Inicia sesión nuevamente.";
  }

  if (error.response?.status === 400) {
    return "El archivo no cumple con las reglas del sistema.";
  }

  if (
    error.response?.status === 413
  ) {
    return "El archivo supera el tamaño permitido.";
  }

  if (
    error.response?.status === 409
  ) {
    return "El archivo no puede procesarse por un conflicto.";
  }

  return "No fue posible completar la operación.";
}

function isImage(
  media: MediaItem,
): boolean {
  return (
    media.mediaType
      .toLowerCase() === "image" ||
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
      <div className="media-card__placeholder">
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
  const [media, setMedia] =
    useState<MediaItem[]>([]);

  const [folders, setFolders] =
    useState<MediaFolder[]>([]);

  const [filter, setFilter] =
    useState<MediaFilter>("all");

  const [isLoading, setIsLoading] =
    useState(true);

  const [isSubmitting, setIsSubmitting] =
    useState(false);

  const [isFormVisible, setIsFormVisible] =
    useState(false);

  const [errorMessage, setErrorMessage] =
    useState("");

  const loadData = useCallback(async () => {
    setIsLoading(true);
    setErrorMessage("");

    try {
      const [
        mediaData,
        folderData,
      ] = await Promise.all([
        mediaService.getAll(),
        mediaFoldersService.getAll(),
      ]);

      setMedia(mediaData);
      setFolders(folderData);
    } catch (error) {
      setErrorMessage(
        getErrorMessage(error),
      );
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

  const visibleMedia = useMemo(
    () =>
      media.filter((item) => {
        if (filter === "all") {
          return true;
        }

        const itemIsImage =
          isImage(item);

        return filter === "image"
          ? itemIsImage
          : !itemIsImage;
      }),
    [filter, media],
  );

  const getFolderName = (
    folderId: string | null,
  ): string => {
    if (!folderId) {
      return "Sin carpeta";
    }

    return (
      folders.find(
        (folder) =>
          folder.id === folderId,
      )?.name ?? "Carpeta no disponible"
    );
  };

  const handleUpload = async (
    request: UploadMediaRequest,
  ) => {
    setIsSubmitting(true);
    setErrorMessage("");

    try {
      await mediaService.upload(request);
      await loadData();
      setIsFormVisible(false);
    } catch (error) {
      setErrorMessage(
        getErrorMessage(error),
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDeactivate = async (
    item: MediaItem,
  ) => {
    const confirmed = window.confirm(
      `¿Deseas desactivar "${item.originalFileName}"?`,
    );

    if (!confirmed) {
      return;
    }

    setErrorMessage("");

    try {
      await mediaService.deactivate(
        item.id,
      );

      await loadData();
    } catch (error) {
      setErrorMessage(
        getErrorMessage(error),
      );
    }
  };

  const handleDownload = async (
    item: MediaItem,
  ) => {
    setErrorMessage("");

    try {
      await mediaService.download(item);
    } catch (error) {
      setErrorMessage(
        getErrorMessage(error),
      );
    }
  };

  return (
    <section>
      <div className="page-heading">
        <div>
          <p className="page-heading__eyebrow">
            Biblioteca multimedia
          </p>

          <h2>Archivos multimedia</h2>

          <p>
            Administra las imágenes y
            videos usados en las playlists.
          </p>
        </div>

        <div className="page-heading__actions">
          <div className="media-filter">
            <label htmlFor="media-filter">
              Mostrar
            </label>

            <select
              id="media-filter"
              value={filter}
              onChange={(event) =>
                setFilter(
                  event.target
                    .value as MediaFilter,
                )
              }
            >
              <option value="all">
                Todos
              </option>

              <option value="image">
                Imágenes
              </option>

              <option value="video">
                Videos
              </option>
            </select>
          </div>

          <button
            type="button"
            className="button button--secondary"
            onClick={() =>
              void loadData()
            }
          >
            <RefreshCw
              size={18}
              aria-hidden="true"
            />
            Actualizar
          </button>

          <button
            type="button"
            className="button button--primary"
            onClick={() =>
              setIsFormVisible(true)
            }
          >
            <Plus
              size={18}
              aria-hidden="true"
            />
            Subir archivo
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
              Subir contenido multimedia
            </h3>

            <p>
              Selecciona un archivo y una
              carpeta opcional.
            </p>
          </div>

          <MediaUploadForm
            folders={folders}
            isSubmitting={isSubmitting}
            onSubmit={handleUpload}
            onCancel={() =>
              setIsFormVisible(false)
            }
          />
        </section>
      )}

      <section className="panel">
        {isLoading ? (
          <LoadingState
            message="Cargando contenido..."
          />
        ) : visibleMedia.length === 0 ? (
          <EmptyState
            title="No hay contenido"
            description="Sube una imagen o video para comenzar."
          />
        ) : (
          <div className="media-grid">
            {visibleMedia.map((item) => {
              const itemIsImage =
                isImage(item);

              return (
                <article
                  className="media-card"
                  key={item.id}
                >
                  <div className="media-card__preview">
                    {itemIsImage ? (
                      <MediaImagePreview item={item} />
                    ) : (
                      <div className="media-card__placeholder">
                        <FileVideo
                          size={42}
                          aria-hidden="true"
                        />
                        <span>Video</span>
                      </div>
                    )}
                  </div>

                  <div className="media-card__content">
                    <div className="media-card__type">
                      {itemIsImage ? (
                        <FileImage
                          size={17}
                          aria-hidden="true"
                        />
                      ) : (
                        <FileVideo
                          size={17}
                          aria-hidden="true"
                        />
                      )}

                      <span>
                        {itemIsImage
                          ? "Imagen"
                          : "Video"}
                      </span>
                    </div>

                    <h3 title={item.originalFileName}>
                      {item.originalFileName}
                    </h3>

                    <dl className="media-card__details">
                      <div>
                        <dt>Tamaño</dt>
                        <dd>
                          {formatFileSize(
                            item.fileSizeBytes,
                          )}
                        </dd>
                      </div>

                      <div>
                        <dt>Carpeta</dt>
                        <dd>
                          {item.mediaFolderName ??
                            getFolderName(
                              item.mediaFolderId,
                            )}
                        </dd>
                      </div>

                      {!itemIsImage && (
                        <div>
                          <dt>Duración</dt>
                          <dd>
                            {formatDuration(
                              item.durationSeconds,
                            )}
                          </dd>
                        </div>
                      )}

                      <div>
                        <dt>Subido</dt>
                        <dd>
                          {formatDateTime(
                            item.uploadedAt,
                          )}
                        </dd>
                      </div>
                    </dl>
                  </div>

                  <div className="media-card__actions">
                    <button
                      type="button"
                      className="icon-button"
                      title="Descargar archivo"
                      onClick={() =>
                        void handleDownload(
                          item,
                        )
                      }
                    >
                      <Download
                        size={17}
                        aria-hidden="true"
                      />
                    </button>

                    <button
                      type="button"
                      className="icon-button icon-button--danger"
                      title="Desactivar archivo"
                      onClick={() =>
                        void handleDeactivate(
                          item,
                        )
                      }
                    >
                      <Trash2
                        size={17}
                        aria-hidden="true"
                      />
                    </button>
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
