import axios from "axios";
import { ChevronLeft, ChevronRight, Search, X } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { EmptyState } from "../../components/common/EmptyState";
import { LoadingState } from "../../components/common/LoadingState";
import { MediaThumbnail } from "../media/MediaThumbnail";
import { mediaService } from "../media/media.service";
import type { MediaItem, MediaSort } from "../media/media.types";
import { mediaFoldersService } from "../mediaFolders/mediaFolders.service";
import type { MediaFolder } from "../mediaFolders/mediaFolders.types";

interface PlaylistMediaPickerModalProps {
  onClose: () => void;
  onAdd: (media: MediaItem[], durationSeconds: number | null) => void;
}

type MediaFilter = "all" | "image" | "video";

const PAGE_SIZE = 20;

function getErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error) && error.response?.status === 401) {
    return "La sesión expiró. Inicia sesión nuevamente.";
  }
  return "No fue posible cargar los archivos multimedia.";
}

export function PlaylistMediaPickerModal({
  onClose,
  onAdd,
}: PlaylistMediaPickerModalProps) {
  const [media, setMedia] = useState<MediaItem[]>([]);
  const [folders, setFolders] = useState<MediaFolder[]>([]);
  const [selectedMedia, setSelectedMedia] = useState<Map<string, MediaItem>>(
    () => new Map(),
  );
  const [searchInput, setSearchInput] = useState("");
  const [searchTerm, setSearchTerm] = useState("");
  const [mediaFilter, setMediaFilter] = useState<MediaFilter>("all");
  const [sort, setSort] = useState<MediaSort>("recent");
  const [folderFilter, setFolderFilter] = useState("all");
  const [duration, setDuration] = useState("");
  const [page, setPage] = useState(1);
  const [totalItems, setTotalItems] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");

  const selectedItems = useMemo(
    () => [...selectedMedia.values()],
    [selectedMedia],
  );
  const parsedDuration = duration ? Number(duration) : null;
  const durationIsValid =
    parsedDuration === null
    || (parsedDuration > 0 && parsedDuration <= 2147483647);

  useEffect(() => {
    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") onClose();
    };
    document.addEventListener("keydown", handleKeyDown);
    return () => {
      document.body.style.overflow = previousOverflow;
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [onClose]);

  useEffect(() => {
    const timeout = window.setTimeout(() => {
      const nextSearchTerm = searchInput.trim();
      if (nextSearchTerm !== searchTerm) {
        setIsLoading(true);
        setErrorMessage("");
        setSearchTerm(nextSearchTerm);
        setPage(1);
      }
    }, 300);
    return () => window.clearTimeout(timeout);
  }, [searchInput, searchTerm]);

  useEffect(() => {
    let cancelled = false;
    void mediaFoldersService.getAll()
      .then((result) => {
        if (!cancelled) setFolders(result);
      })
      .catch(() => {
        if (!cancelled) setErrorMessage("No fue posible cargar las carpetas.");
      });
    return () => { cancelled = true; };
  }, []);

  useEffect(() => {
    let cancelled = false;

    void mediaService.getPaged({
      page,
      pageSize: PAGE_SIZE,
      search: searchTerm,
      mediaType: mediaFilter,
      mediaFolderId:
        folderFilter !== "all" && folderFilter !== "root"
          ? folderFilter
          : null,
      rootOnly: folderFilter === "root",
      sort,
    }).then((result) => {
      if (cancelled) return;
      setMedia(result.items);
      setTotalItems(result.totalItems);
      setTotalPages(result.totalPages);
      if (result.totalPages > 0 && page > result.totalPages) {
        setPage(result.totalPages);
      }
    }).catch((error: unknown) => {
      if (!cancelled) setErrorMessage(getErrorMessage(error));
    }).finally(() => {
      if (!cancelled) setIsLoading(false);
    });

    return () => { cancelled = true; };
  }, [folderFilter, mediaFilter, page, searchTerm, sort]);

  const toggleMedia = (item: MediaItem) => {
    setSelectedMedia((current) => {
      const next = new Map(current);
      if (next.has(item.id)) next.delete(item.id);
      else next.set(item.id, item);
      return next;
    });
  };

  return (
    <div
      className="media-modal-backdrop"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) onClose();
      }}
    >
      <section
        className="playlist-media-picker"
        role="dialog"
        aria-modal="true"
        aria-labelledby="playlist-media-picker-title"
      >
        <header className="playlist-media-picker__header">
          <div>
            <span>Biblioteca multimedia</span>
            <h3 id="playlist-media-picker-title">Agregar contenido</h3>
          </div>
          <button type="button" className="icon-button" aria-label="Cerrar" onClick={onClose}>
            <X size={20} aria-hidden="true" />
          </button>
        </header>

        <div className="playlist-media-picker__filters">
          <label className="playlist-media-picker__search">
            <span className="sr-only">Buscar archivos</span>
            <Search size={18} aria-hidden="true" />
            <input
              value={searchInput}
              onChange={(event) => setSearchInput(event.target.value)}
              placeholder="Buscar por nombre..."
              autoFocus
            />
          </label>

          <select
            aria-label="Filtrar por tipo"
            value={mediaFilter}
            onChange={(event) => {
              setIsLoading(true);
              setErrorMessage("");
              setMediaFilter(event.target.value as MediaFilter);
              setPage(1);
            }}
          >
            <option value="all">Todos los tipos</option>
            <option value="image">Imágenes</option>
            <option value="video">Videos</option>
          </select>

          <select
            aria-label="Filtrar por carpeta"
            value={folderFilter}
            onChange={(event) => {
              setIsLoading(true);
              setErrorMessage("");
              setFolderFilter(event.target.value);
              setPage(1);
            }}
          >
            <option value="all">Todas las carpetas</option>
            <option value="root">Raíz</option>
            {folders.map((folder) => (
              <option key={folder.id} value={folder.id}>{folder.name}</option>
            ))}
          </select>

          <select
            aria-label="Ordenar archivos"
            value={sort}
            onChange={(event) => {
              setIsLoading(true);
              setErrorMessage("");
              setSort(event.target.value as MediaSort);
              setPage(1);
            }}
          >
            <option value="recent">Más recientes</option>
            <option value="nameAsc">Nombre A–Z</option>
            <option value="nameDesc">Nombre Z–A</option>
          </select>
        </div>

        {errorMessage && <div className="alert alert--error" role="alert">{errorMessage}</div>}

        <div className="playlist-media-picker__content">
          {isLoading ? (
            <LoadingState message="Cargando archivos..." />
          ) : media.length === 0 ? (
            <EmptyState title="No hay archivos" description="No se encontraron archivos para los filtros seleccionados." />
          ) : (
            <div className="playlist-media-picker__grid">
              {media.map((item) => {
                const isSelected = selectedMedia.has(item.id);
                return (
                  <button
                    type="button"
                    key={item.id}
                    className={`playlist-media-option${isSelected ? " playlist-media-option--selected" : ""}`}
                    aria-pressed={isSelected}
                    onClick={() => toggleMedia(item)}
                  >
                    <span className="playlist-media-option__thumbnail">
                      <MediaThumbnail item={item} />
                      <input type="checkbox" tabIndex={-1} checked={isSelected} readOnly aria-hidden="true" />
                    </span>
                    <strong title={item.originalFileName}>{item.originalFileName}</strong>
                    <small>{item.mediaType === "Image" ? "Imagen" : "Video"}</small>
                  </button>
                );
              })}
            </div>
          )}
        </div>

        <footer className="playlist-media-picker__footer">
          <div className="playlist-media-picker__summary">
            <strong>{selectedItems.length} seleccionado{selectedItems.length === 1 ? "" : "s"}</strong>
            <label>
              Duración individual
              <input
                type="text"
                inputMode="numeric"
                pattern="[0-9]*"
                maxLength={10}
                value={duration}
                onChange={(event) => setDuration(event.target.value.replace(/\D/g, ""))}
                placeholder="Pred. 10 seg."
              />
            </label>
          </div>

          <div className="playlist-media-picker__pagination">
            <span>{totalItems} archivo{totalItems === 1 ? "" : "s"}</span>
            <button type="button" className="icon-button" aria-label="Página anterior" disabled={page <= 1} onClick={() => {
              setIsLoading(true);
              setErrorMessage("");
              setPage((value) => value - 1);
            }}>
              <ChevronLeft size={18} aria-hidden="true" />
            </button>
            <span>Página {page} de {Math.max(totalPages, 1)}</span>
            <button type="button" className="icon-button" aria-label="Página siguiente" disabled={page >= totalPages} onClick={() => {
              setIsLoading(true);
              setErrorMessage("");
              setPage((value) => value + 1);
            }}>
              <ChevronRight size={18} aria-hidden="true" />
            </button>
          </div>

          <div className="form-actions">
            <button type="button" className="button button--secondary" onClick={onClose}>Cancelar</button>
            <button
              type="button"
              className="button button--primary"
              disabled={selectedItems.length === 0 || !durationIsValid}
              onClick={() => onAdd(selectedItems, parsedDuration)}
            >
              Agregar {selectedItems.length || ""} elemento{selectedItems.length === 1 ? "" : "s"}
            </button>
          </div>
        </footer>
      </section>
    </div>
  );
}
