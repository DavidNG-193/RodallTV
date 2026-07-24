import axios from "axios";
import {
  ListVideo,
  Pencil,
  Plus,
  RefreshCw,
  Trash2,
} from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { EmptyState } from "../components/common/EmptyState";
import { LoadingState } from "../components/common/LoadingState";
import { PlaylistForm } from "../features/playlist/PlaylistForm";
import { playlistsService } from "../features/playlist/playlists.service";
import type {
  CreatePlaylistRequest,
  Playlist,
  UpdatePlaylistRequest,
} from "../features/playlist/playlists.types";
import { formatDateTime } from "../utils/fileFormatters";

function getErrorMessage(error: unknown): string {
  if (!axios.isAxiosError(error)) {
    return "Ocurrió un error inesperado.";
  }

  if (error.response?.status === 400) {
    return "Los datos de la playlist no son válidos.";
  }

  if (error.response?.status === 401) {
    return "La sesión expiró. Inicia sesión nuevamente.";
  }

  if (error.response?.status === 409) {
    return "Ya existe una playlist con ese nombre.";
  }

  return "No fue posible completar la operación.";
}

export function PlaylistsPage() {
  const navigate = useNavigate();
  const [playlists, setPlaylists] = useState<Playlist[]>([]);
  const [selectedPlaylist, setSelectedPlaylist] =
    useState<Playlist | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isFormVisible, setIsFormVisible] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  const loadPlaylists = useCallback(async () => {
    setIsLoading(true);
    setErrorMessage("");

    try {
      const data = await playlistsService.getAll();
      setPlaylists(data);
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    let isCancelled = false;

    void playlistsService
      .getAll()
      .then((data) => {
        if (!isCancelled) {
          setPlaylists(data);
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

  const openCreateForm = () => {
    setSelectedPlaylist(null);
    setErrorMessage("");
    setIsFormVisible(true);
  };

  const openEditForm = (playlist: Playlist) => {
    setSelectedPlaylist(playlist);
    setErrorMessage("");
    setIsFormVisible(true);
  };

  const closeForm = () => {
    setSelectedPlaylist(null);
    setIsFormVisible(false);
  };

  const handleSubmit = async (
    request: CreatePlaylistRequest | UpdatePlaylistRequest,
  ) => {
    setIsSubmitting(true);
    setErrorMessage("");

    try {
      if (selectedPlaylist) {
        await playlistsService.update(
          selectedPlaylist.id,
          request as UpdatePlaylistRequest,
        );
      } else {
        await playlistsService.create(
          request as CreatePlaylistRequest,
        );
      }

      await loadPlaylists();
      closeForm();
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDeactivate = async (playlist: Playlist) => {
    const confirmed = window.confirm(
      `¿Deseas desactivar la playlist "${playlist.name}"?`,
    );

    if (!confirmed) {
      return;
    }

    setErrorMessage("");

    try {
      await playlistsService.deactivate(playlist.id);
      await loadPlaylists();
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    }
  };

  return (
    <section>
      <div className="page-heading">
        <div>
          <p className="page-heading__eyebrow">Programación</p>
          <h2>Playlists</h2>
          <p>
            Organiza el contenido que se mostrará en las pantallas.
          </p>
        </div>

        <div className="page-heading__actions">
          <button
            type="button"
            className="button button--secondary"
            onClick={() => void loadPlaylists()}
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
            Nueva playlist
          </button>
        </div>
      </div>

      {errorMessage && (
        <div className="alert alert--error" role="alert">
          {errorMessage}
        </div>
      )}

      {isFormVisible && (
        <section className="panel">
          <div className="panel__heading">
            <h3>
              {selectedPlaylist
                ? "Editar playlist"
                : "Crear playlist"}
            </h3>
            <p>Define un nombre y una descripción opcional.</p>
          </div>

          <PlaylistForm
            playlist={selectedPlaylist}
            isSubmitting={isSubmitting}
            onSubmit={handleSubmit}
            onCancel={closeForm}
          />
        </section>
      )}

      <section className="panel">
        {isLoading ? (
          <LoadingState message="Cargando playlists..." />
        ) : playlists.length === 0 ? (
          <EmptyState
            title="No hay playlists"
            description="Crea una playlist vacía para comenzar."
          />
        ) : (
          <div className="playlist-grid">
            {playlists.map((playlist) => (
              <article className="playlist-card" key={playlist.id}>
                <div className="playlist-card__heading">
                  <ListVideo size={24} aria-hidden="true" />
                  <h3>{playlist.name}</h3>
                </div>

                <p>{playlist.description ?? "Sin descripción"}</p>

                <div className="playlist-card__meta">
                  <span>Versión {playlist.version}</span>
                  <span
                    className={
                      playlist.isActive
                        ? "record-status record-status--active"
                        : "record-status record-status--inactive"
                    }
                  >
                    {playlist.isActive ? "Activa" : "Inactiva"}
                  </span>
                </div>

                <small>
                  Actualizada el {formatDateTime(playlist.updatedAt)}
                </small>

                <div className="playlist-card__actions">
                  <button
                    type="button"
                    className="button button--secondary"
                    onClick={() =>
                      navigate(`/playlists/${playlist.id}`)
                    }
                  >
                    <ListVideo size={17} aria-hidden="true" />
                    Contenido
                  </button>

                  <button
                    type="button"
                    className="icon-button"
                    title="Editar playlist"
                    onClick={() => openEditForm(playlist)}
                  >
                    <Pencil size={17} aria-hidden="true" />
                  </button>

                  <button
                    type="button"
                    className="icon-button icon-button--danger"
                    title="Desactivar playlist"
                    onClick={() => void handleDeactivate(playlist)}
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
