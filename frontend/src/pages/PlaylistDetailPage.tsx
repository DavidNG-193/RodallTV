import axios from "axios";
import {
  ArrowDown,
  ArrowLeft,
  ArrowUp,
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
import { useNavigate, useParams } from "react-router-dom";
import { EmptyState } from "../components/common/EmptyState";
import { LoadingState } from "../components/common/LoadingState";
import type { MediaItem } from "../features/media/media.types";
import { mediaService } from "../features/media/media.service";
import { PlaylistItemForm } from "../features/playlist/PlaylistItemForm";
import { playlistsService } from "../features/playlist/playlists.service";
import type {
  CreatePlaylistItemRequest,
  Playlist,
  PlaylistItem,
  UpdatePlaylistItemRequest,
} from "../features/playlist/playlists.types";
import { PERMISSIONS } from "../features/auth/permission.constants";
import { useAuth } from "../features/auth/useAuth";

interface PlaylistDetailData {
  playlist: Playlist;
  items: PlaylistItem[];
  media: MediaItem[];
}

function getErrorMessage(error: unknown): string {
  if (!axios.isAxiosError(error)) {
    return "Ocurrió un error inesperado.";
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
  includeManagementData: boolean,
): Promise<PlaylistDetailData> {
  const [playlist, items, media] = await Promise.all([
    playlistsService.getById(playlistId),
    playlistsService.getItems(playlistId),
    includeManagementData ? mediaService.getAll() : Promise.resolve([]),
  ]);

  return {
    playlist,
    items: [...items].sort(
      (first, second) => first.position - second.position,
    ),
    media,
  };
}

export function PlaylistDetailPage() {
  const { hasPermission } = useAuth();
  const canManagePlaylists = hasPermission(PERMISSIONS.PLAYLISTS_MANAGE);
  const { playlistId } = useParams<{ playlistId: string }>();
  const navigate = useNavigate();
  const [playlist, setPlaylist] = useState<Playlist | null>(null);
  const [items, setItems] = useState<PlaylistItem[]>([]);
  const [media, setMedia] = useState<MediaItem[]>([]);
  const [selectedItem, setSelectedItem] =
    useState<PlaylistItem | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isFormVisible, setIsFormVisible] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  const applyData = useCallback((data: PlaylistDetailData) => {
    setPlaylist(data.playlist);
    setItems(data.items);
    setMedia(data.media);
  }, []);

  const loadData = useCallback(async () => {
    if (!playlistId) {
      return;
    }

    setIsLoading(true);
    setErrorMessage("");

    try {
      applyData(await fetchPlaylistDetail(playlistId, canManagePlaylists));
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsLoading(false);
    }
  }, [applyData, canManagePlaylists, playlistId]);

  useEffect(() => {
    if (!playlistId) {
      return;
    }

    let isCancelled = false;

    void fetchPlaylistDetail(playlistId, canManagePlaylists)
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
  }, [applyData, canManagePlaylists, playlistId]);

  const openAddForm = () => {
    setSelectedItem(null);
    setErrorMessage("");
    setIsFormVisible(true);
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

  const handleSubmit = async (
    request:
      | CreatePlaylistItemRequest
      | UpdatePlaylistItemRequest,
  ) => {
    if (!playlistId) {
      return;
    }

    setIsSubmitting(true);
    setErrorMessage("");

    try {
      if (selectedItem) {
        await playlistsService.updateItem(
          playlistId,
          selectedItem.id,
          request as UpdatePlaylistItemRequest,
        );
      } else {
        await playlistsService.addItem(
          playlistId,
          request as CreatePlaylistItemRequest,
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

  const handleRemove = async (item: PlaylistItem) => {
    if (!playlistId) {
      return;
    }

    const confirmed = window.confirm(
      `¿Deseas eliminar "${item.originalFileName}" de la playlist?`,
    );

    if (!confirmed) {
      return;
    }

    setIsSubmitting(true);
    setErrorMessage("");

    try {
      await playlistsService.removeItem(playlistId, item.id);
      await loadData();
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsSubmitting(false);
    }
  };

  const moveItem = async (
    currentIndex: number,
    direction: -1 | 1,
  ) => {
    if (!playlistId) {
      return;
    }

    const destinationIndex = currentIndex + direction;

    if (
      destinationIndex < 0 ||
      destinationIndex >= items.length
    ) {
      return;
    }

    const previousItems = items;
    const reordered = [...items];

    [reordered[currentIndex], reordered[destinationIndex]] = [
      reordered[destinationIndex],
      reordered[currentIndex],
    ];

    setItems(reordered);
    setIsSubmitting(true);
    setErrorMessage("");

    try {
      await playlistsService.reorderItems(playlistId, {
        orderedItemIds: reordered.map((item) => item.id),
      });

      await loadData();
    } catch (error) {
      setItems(previousItems);
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsSubmitting(false);
    }
  };

  const getMediaName = (item: PlaylistItem): string =>
    item.originalFileName ||
    media.find((mediaItem) => mediaItem.id === item.mediaId)
      ?.originalFileName ||
    "Archivo no disponible";

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
          </p>
        </div>

        <div className="page-heading__actions">
          <button
            type="button"
            className="button button--secondary"
            onClick={() => navigate("/playlists")}
          >
            <ArrowLeft size={18} aria-hidden="true" />
            Volver
          </button>

          <button
            type="button"
            className="button button--secondary"
            onClick={() => void loadData()}
          >
            <RefreshCw size={18} aria-hidden="true" />
            Actualizar
          </button>

          {canManagePlaylists && <button
            type="button"
            className="button button--primary"
            onClick={openAddForm}
            disabled={!playlist}
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

      {canManagePlaylists && isFormVisible && (
        <section className="panel">
          <div className="panel__heading">
            <h3>
              {selectedItem
                ? "Editar duración"
                : "Agregar contenido"}
            </h3>
            <p>
              La duración personalizada es opcional y debe ser mayor
              que cero.
            </p>
          </div>

          <PlaylistItemForm
            media={media}
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
          <div className="playlist-item-list">
            {items.map((item, index) => (
              <article className="playlist-item-row" key={item.id}>
                <span className="playlist-item-row__position">
                  {index + 1}
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
                    title="Mover arriba"
                    disabled={isSubmitting || index === 0}
                    onClick={() => void moveItem(index, -1)}
                  >
                    <ArrowUp size={17} aria-hidden="true" />
                  </button>

                  <button
                    type="button"
                    className="icon-button"
                    title="Mover abajo"
                    disabled={
                      isSubmitting || index === items.length - 1
                    }
                    onClick={() => void moveItem(index, 1)}
                  >
                    <ArrowDown size={17} aria-hidden="true" />
                  </button>

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
    </section>
  );
}
