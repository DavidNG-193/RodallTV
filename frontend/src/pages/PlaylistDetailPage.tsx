import axios from "axios";
import {
  ArrowLeft,
  GripVertical,
  Pencil,
  Plus,
  RefreshCw,
  Save,
  Trash2,
  Undo2,
} from "lucide-react";
import {
  useCallback,
  useEffect,
  useState,
} from "react";
import { useNavigate, useParams } from "react-router-dom";
import { EmptyState } from "../components/common/EmptyState";
import { LoadingState } from "../components/common/LoadingState";
import type { MediaItem } from "../features/media/media.types";
import { MediaThumbnail } from "../features/media/MediaThumbnail";
import { PlaylistItemForm } from "../features/playlist/PlaylistItemForm";
import { PlaylistMediaPickerModal } from "../features/playlist/PlaylistMediaPickerModal";
import { playlistsService } from "../features/playlist/playlists.service";
import type {
  Playlist,
  PlaylistItem,
  UpdatePlaylistItemRequest,
} from "../features/playlist/playlists.types";
import { PERMISSIONS } from "../features/auth/permission.constants";
import { useAuth } from "../features/auth/useAuth";

interface PlaylistDetailData {
  playlist: Playlist;
  items: PlaylistItem[];
}

interface DraftPlaylistItem extends PlaylistItem {
  isNew?: boolean;
}

function getErrorMessage(error: unknown): string {
  if (!axios.isAxiosError(error)) {
    return "Ocurrió un error inesperado.";
  }

  const apiMessage = error.response?.data?.message;
  if (typeof apiMessage === "string" && apiMessage.trim()) {
    return apiMessage;
  }

  if (error.response?.status === 400) {
    return "Los datos enviados no son válidos.";
  }

  if (error.response?.status === 401) {
    return "La sesión expiró. Inicia sesión nuevamente.";
  }

  if (error.response?.status === 404) {
    return "La playlist o el elemento solicitado no existe.";
  }

  if (error.response?.status === 409) {
    return "No fue posible aplicar el cambio por un conflicto.";
  }

  return "No fue posible completar la operación.";
}

async function fetchPlaylistDetail(
  playlistId: string,
): Promise<PlaylistDetailData> {
  const [playlist, items] = await Promise.all([
    playlistsService.getById(playlistId),
    playlistsService.getItems(playlistId),
  ]);

  return {
    playlist,
    items: [...items].sort(
      (first, second) => first.position - second.position,
    ),
  };
}

export function PlaylistDetailPage() {
  const { hasPermission } = useAuth();
  const canManagePlaylists = hasPermission(PERMISSIONS.PLAYLISTS_MANAGE);
  const { playlistId } = useParams<{ playlistId: string }>();
  const navigate = useNavigate();
  const [playlist, setPlaylist] = useState<Playlist | null>(null);
  const [items, setItems] = useState<DraftPlaylistItem[]>([]);
  const [selectedItem, setSelectedItem] =
    useState<PlaylistItem | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isFormVisible, setIsFormVisible] = useState(false);
  const [isMediaPickerVisible, setIsMediaPickerVisible] = useState(false);
  const [hasPendingChanges, setHasPendingChanges] = useState(false);
  const [draggedItemId, setDraggedItemId] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState("");

  const applyData = useCallback((data: PlaylistDetailData) => {
    setPlaylist(data.playlist);
    setItems(data.items);
    setHasPendingChanges(false);
    setDraggedItemId(null);
  }, []);

  const loadData = useCallback(async () => {
    if (!playlistId) {
      return;
    }

    setIsLoading(true);
    setErrorMessage("");

    try {
      applyData(await fetchPlaylistDetail(playlistId));
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsLoading(false);
    }
  }, [applyData, playlistId]);

  useEffect(() => {
    if (!playlistId) {
      return;
    }

    let isCancelled = false;

    void fetchPlaylistDetail(playlistId)
      .then((data) => {
        if (!isCancelled) {
          applyData(data);
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
  }, [applyData, playlistId]);

  useEffect(() => {
    if (!hasPendingChanges) return;

    const warnBeforeUnload = (event: BeforeUnloadEvent) => {
      event.preventDefault();
    };
    window.addEventListener("beforeunload", warnBeforeUnload);
    return () => window.removeEventListener("beforeunload", warnBeforeUnload);
  }, [hasPendingChanges]);

  const handleBack = () => {
    if (
      hasPendingChanges
      && !window.confirm("Hay cambios sin guardar. ¿Deseas descartarlos y salir?")
    ) {
      return;
    }
    navigate("/playlists");
  };

  const openAddForm = () => {
    setSelectedItem(null);
    setErrorMessage("");
    setIsMediaPickerVisible(true);
  };

  const openEditForm = (item: PlaylistItem) => {
    setSelectedItem(item);
    setErrorMessage("");
    setIsFormVisible(true);
  };

  const closeForm = () => {
    setSelectedItem(null);
    setIsFormVisible(false);
  };

  const handleSubmit = async (request: UpdatePlaylistItemRequest) => {
    if (!selectedItem) {
      return;
    }

    if (selectedItem.customDurationSeconds !== request.customDurationSeconds) {
      setItems((current) => current.map((item) =>
        item.id === selectedItem.id
          ? { ...item, customDurationSeconds: request.customDurationSeconds ?? null }
          : item,
      ));
      setHasPendingChanges(true);
    }
    closeForm();
  };

  const handleAddMedia = (
    selectedMedia: MediaItem[],
    customDurationSeconds: number | null,
  ) => {
    const createdAt = new Date().toISOString();
    setItems((current) => [
      ...current,
      ...selectedMedia.map((mediaItem, index): DraftPlaylistItem => ({
        id: crypto.randomUUID(),
        playlistId: playlistId ?? "",
        mediaId: mediaItem.id,
        originalFileName: mediaItem.originalFileName,
        storedFileName: mediaItem.storedFileName,
        mediaType: mediaItem.mediaType,
        mimeType: mediaItem.mimeType,
        position: current.length + index + 1,
        customDurationSeconds,
        createdAt,
        isNew: true,
      })),
    ]);
    setHasPendingChanges(true);
    setIsMediaPickerVisible(false);
  };

  const handleRemove = async (item: PlaylistItem) => {
    const confirmed = window.confirm(
      `¿Deseas eliminar "${item.originalFileName}" de la playlist?`,
    );

    if (!confirmed) {
      return;
    }

    setItems((current) => current.filter((currentItem) => currentItem.id !== item.id));
    setHasPendingChanges(true);
  };

  const moveItemLocally = (
    sourceId: string,
    destinationId: string,
    placeAfterDestination: boolean,
  ) => {
    if (sourceId === destinationId) return;

    const sourceIndex = items.findIndex((item) => item.id === sourceId);
    const destinationIndex = items.findIndex((item) => item.id === destinationId);
    if (sourceIndex < 0 || destinationIndex < 0) return;

    let insertionIndex = destinationIndex + (placeAfterDestination ? 1 : 0);
    if (sourceIndex < insertionIndex) insertionIndex -= 1;
    if (sourceIndex === insertionIndex) return;

    const reordered = [...items];
    const [movedItem] = reordered.splice(sourceIndex, 1);
    reordered.splice(insertionIndex, 0, movedItem);
    setItems(reordered);
    setHasPendingChanges(true);
  };

  const handleSaveChanges = async () => {
    if (!playlistId || !playlist || !hasPendingChanges) return;

    setIsSubmitting(true);
    setErrorMessage("");

    try {
      const result = await playlistsService.saveComposition(playlistId, {
        expectedVersion: playlist.version,
        items: items.map((item) => ({
          id: item.isNew ? null : item.id,
          mediaId: item.mediaId,
          customDurationSeconds: item.customDurationSeconds,
        })),
      });
      setItems(result.items);
      setPlaylist({ ...playlist, version: result.version });
      setHasPendingChanges(false);
      setDraggedItemId(null);
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsSubmitting(false);
    }
  };

  const getMediaName = (item: PlaylistItem): string =>
    item.originalFileName || "Archivo no disponible";

  if (!playlistId) {
    return (
      <div className="alert alert--error" role="alert">
        No se proporcionó un identificador de playlist.
      </div>
    );
  }

  return (
    <section>
      <div className="page-heading">
        <div>
          <p className="page-heading__eyebrow">Contenido de playlist</p>
          <h2>{playlist?.name ?? "Playlist"}</h2>
          <p>
            {playlist?.description ?? "Sin descripción"} · Versión{" "}
            {playlist?.version ?? "—"}
            {hasPendingChanges && <span className="playlist-order-status">Cambios sin guardar</span>}
          </p>
        </div>

        <div className="page-heading__actions">
          <button
            type="button"
            className="button button--secondary"
            onClick={handleBack}
          >
            <ArrowLeft size={18} aria-hidden="true" />
            Volver
          </button>

          <button
            type="button"
            className="button button--secondary"
            onClick={() => void loadData()}
            disabled={hasPendingChanges || isSubmitting}
          >
            <RefreshCw size={18} aria-hidden="true" />
            Actualizar
          </button>

          {canManagePlaylists && hasPendingChanges && <button
            type="button"
            className="button button--secondary"
            onClick={() => void loadData()}
            disabled={isSubmitting}
          >
            <Undo2 size={18} aria-hidden="true" />
            Descartar cambios
          </button>}

          {canManagePlaylists && <button
            type="button"
            className="button button--primary"
            onClick={() => void handleSaveChanges()}
            disabled={!hasPendingChanges || isSubmitting}
          >
            <Save size={18} aria-hidden="true" />
            {isSubmitting && hasPendingChanges ? "Guardando..." : "Guardar cambios"}
          </button>}

          {canManagePlaylists && <button
            type="button"
            className="button button--secondary"
            onClick={openAddForm}
            disabled={!playlist || isSubmitting}
          >
            <Plus size={18} aria-hidden="true" />
            Agregar contenido
          </button>}
        </div>
      </div>

      {errorMessage && (
        <div className="alert alert--error" role="alert">
          {errorMessage}
        </div>
      )}

      {canManagePlaylists && isFormVisible && selectedItem && (
        <section className="panel">
          <div className="panel__heading">
            <h3>
              Editar duración
            </h3>
            <p>
              La duración personalizada es opcional y debe ser mayor
              que cero.
            </p>
          </div>

          <PlaylistItemForm
            item={selectedItem}
            isSubmitting={isSubmitting}
            onSubmit={handleSubmit}
            onCancel={closeForm}
          />
        </section>
      )}

      <section className="panel">
        {isLoading ? (
          <LoadingState message="Cargando contenido de la playlist..." />
        ) : items.length === 0 ? (
          <EmptyState
            title="La playlist está vacía"
            description="Agrega imágenes o videos para comenzar."
          />
        ) : (
          <div className={`playlist-item-list${hasPendingChanges ? " playlist-item-list--pending" : ""}`}>
            {items.map((item, index) => (
              <article
                className={`playlist-item-row${draggedItemId === item.id ? " playlist-item-row--dragging" : ""}`}
                key={item.id}
                draggable={canManagePlaylists && !isSubmitting}
                onDragStart={(event) => {
                  setDraggedItemId(item.id);
                  event.dataTransfer.effectAllowed = "move";
                  event.dataTransfer.setData("text/plain", item.id);
                }}
                onDragOver={(event) => {
                  if (!canManagePlaylists || !draggedItemId) return;
                  event.preventDefault();
                  event.dataTransfer.dropEffect = "move";
                  const bounds = event.currentTarget.getBoundingClientRect();
                  moveItemLocally(
                    draggedItemId,
                    item.id,
                    event.clientY > bounds.top + bounds.height / 2,
                  );
                }}
                onDrop={(event) => {
                  event.preventDefault();
                  setDraggedItemId(null);
                }}
                onDragEnd={() => setDraggedItemId(null)}
              >
                {canManagePlaylists && <span className="playlist-item-row__drag" title="Arrastra para cambiar la posición" aria-hidden="true"><GripVertical size={20} /></span>}

                <span className="playlist-item-row__thumbnail">
                  <MediaThumbnail item={{ id: item.mediaId, mediaType: item.mediaType }} />
                  <span className="playlist-item-row__position">{index + 1}</span>
                </span>

                <div className="playlist-item-row__content">
                  <strong>{getMediaName(item)}</strong>
                  <small>
                    {item.mediaType} ·{" "}
                    {item.customDurationSeconds
                      ? `${item.customDurationSeconds} segundos`
                      : "Duración predeterminada"}
                  </small>
                </div>

                {canManagePlaylists && <div className="playlist-item-row__actions">
                  <button
                    type="button"
                    className="icon-button"
                    title="Editar duración"
                    disabled={isSubmitting}
                    onClick={() => openEditForm(item)}
                  >
                    <Pencil size={17} aria-hidden="true" />
                  </button>

                  <button
                    type="button"
                    className="icon-button icon-button--danger"
                    title="Eliminar elemento"
                    disabled={isSubmitting}
                    onClick={() => void handleRemove(item)}
                  >
                    <Trash2 size={17} aria-hidden="true" />
                  </button>
                </div>}
              </article>
            ))}
          </div>
        )}
      </section>

      {canManagePlaylists && isMediaPickerVisible && (
        <PlaylistMediaPickerModal
          onClose={() => setIsMediaPickerVisible(false)}
          onAdd={handleAddMedia}
        />
      )}
    </section>
  );
}
