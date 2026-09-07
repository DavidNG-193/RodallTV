import axios from "axios";
import {
  ChevronLeft, ChevronRight, Download, FileImage, FileVideo, Folder,
  Grid2X2, Home, Info, List as ListIcon, MoreVertical, MoveUp,
  Pencil, Plus, RefreshCw, Search, Trash2, Upload,
} from "lucide-react";
import { useCallback, useEffect, useMemo, useState } from "react";
import { EmptyState } from "../components/common/EmptyState";
import { LoadingState } from "../components/common/LoadingState";
import { PERMISSIONS } from "../features/auth/permission.constants";
import { useAuth } from "../features/auth/useAuth";
import { MediaDetailsModal } from "../features/media/MediaDetailsModal";
import { MediaPreviewModal } from "../features/media/MediaPreviewModal";
import { MediaRenameModal } from "../features/media/MediaRenameModal";
import { MediaThumbnail } from "../features/media/MediaThumbnail";
import { MediaUploadForm } from "../features/media/MediaUploadForm";
import { mediaService } from "../features/media/media.service";
import type { MediaItem, MediaSort, UploadMediaRequest } from "../features/media/media.types";
import { MediaFolderForm } from "../features/mediaFolders/MediaFolderForm";
import { mediaFoldersService } from "../features/mediaFolders/mediaFolders.service";
import type { CreateMediaFolderRequest, MediaFolder, UpdateMediaFolderRequest } from "../features/mediaFolders/mediaFolders.types";
import { formatDateTime, formatFileSize } from "../utils/fileFormatters";

type MediaFilter = "all" | "image" | "video";
type FormMode = "upload" | "folder" | null;
type ViewMode = "grid" | "list";

interface ContextMenuState {
  item: MediaItem;
  left: number;
  top: number;
}

function getErrorMessage(error: unknown): string {
  if (!axios.isAxiosError(error)) return "Ocurrió un error inesperado.";
  const apiMessage = error.response?.data?.message;
  if (typeof apiMessage === "string" && apiMessage.trim()) return apiMessage;
  if (error.response?.status === 401) return "La sesión expiró. Inicia sesión nuevamente.";
  if (error.response?.status === 400) return "Los datos enviados no son válidos.";
  if (error.response?.status === 413) return "El archivo supera el tamaño permitido.";
  if (error.response?.status === 409) return "Ya existe una carpeta con ese nombre.";
  return "No fue posible completar la operación.";
}

function getInitialViewMode(): ViewMode {
  return localStorage.getItem("rodalltv_media_view") === "list" ? "list" : "grid";
}

export function MediaPage() {
  const { hasPermission } = useAuth();
  const canManageMedia = hasPermission(PERMISSIONS.MEDIA_MANAGE);
  const [media, setMedia] = useState<MediaItem[]>([]);
  const [folders, setFolders] = useState<MediaFolder[]>([]);
  const [selectedFolderId, setSelectedFolderId] = useState<string | null>(null);
  const [selectedFolderForEdit, setSelectedFolderForEdit] = useState<MediaFolder | null>(null);
  const [filter, setFilter] = useState<MediaFilter>("all");
  const [sort, setSort] = useState<MediaSort>("recent");
  const [viewMode, setViewMode] = useState<ViewMode>(getInitialViewMode);
  const [searchInput, setSearchInput] = useState("");
  const [searchTerm, setSearchTerm] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(() => getInitialViewMode() === "list" ? 25 : 24);
  const [totalItems, setTotalItems] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [reloadKey, setReloadKey] = useState(0);
  const [formMode, setFormMode] = useState<FormMode>(null);
  const [previewItem, setPreviewItem] = useState<MediaItem | null>(null);
  const [detailsItem, setDetailsItem] = useState<MediaItem | null>(null);
  const [renameItem, setRenameItem] = useState<MediaItem | null>(null);
  const [contextMenu, setContextMenu] = useState<ContextMenuState | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  const selectedFolder = useMemo(
    () => folders.find((folder) => folder.id === selectedFolderId) ?? null,
    [folders, selectedFolderId],
  );
  const closePreview = useCallback(() => setPreviewItem(null), []);
  const loadFolders = useCallback(async () => setFolders(await mediaFoldersService.getAll()), []);

  useEffect(() => {
    const timeout = window.setTimeout(() => {
      const nextSearchTerm = searchInput.trim();

      if (nextSearchTerm !== searchTerm) {
        setSearchTerm(nextSearchTerm);
        setPage(1);
        setIsLoading(true);
      }
    }, 300);
    return () => window.clearTimeout(timeout);
  }, [searchInput, searchTerm]);

  useEffect(() => {
    let cancelled = false;
    void mediaService.getPaged({
      page, pageSize, search: searchTerm, mediaType: filter,
      mediaFolderId: selectedFolderId, rootOnly: selectedFolderId === null,
      sort,
    }).then((result) => {
      if (cancelled) return;
      setMedia(result.items);
      setTotalItems(result.totalItems);
      setTotalPages(result.totalPages);
      if (result.totalPages > 0 && page > result.totalPages) setPage(result.totalPages);
    }).catch((error: unknown) => {
      if (!cancelled) setErrorMessage(getErrorMessage(error));
    }).finally(() => {
      if (!cancelled) setIsLoading(false);
    });
    return () => { cancelled = true; };
  }, [filter, page, pageSize, reloadKey, searchTerm, selectedFolderId, sort]);

  useEffect(() => {
    let cancelled = false;
    void mediaFoldersService.getAll().then((items) => {
      if (!cancelled) setFolders(items);
    }).catch((error: unknown) => {
      if (!cancelled) setErrorMessage(getErrorMessage(error));
    });
    return () => { cancelled = true; };
  }, []);

  useEffect(() => {
    const closeMenu = () => setContextMenu(null);
    document.addEventListener("click", closeMenu);
    window.addEventListener("resize", closeMenu);
    window.addEventListener("scroll", closeMenu, true);
    return () => {
      document.removeEventListener("click", closeMenu);
      window.removeEventListener("resize", closeMenu);
      window.removeEventListener("scroll", closeMenu, true);
    };
  }, []);

  const closeForm = () => { setFormMode(null); setSelectedFolderForEdit(null); };
  const refreshMedia = () => {
    setIsLoading(true);
    setReloadKey((value) => value + 1);
  };

  const openFolder = (folder: MediaFolder) => {
    setIsLoading(true); setSelectedFolderId(folder.id); setPage(1); setFilter("all");
    setSearchInput(""); setSearchTerm(""); setErrorMessage(""); closeForm();
  };
  const goToRoot = () => {
    if (
      selectedFolderId === null &&
      page === 1 &&
      filter === "all" &&
      searchInput === "" &&
      searchTerm === ""
    ) {
      closeForm();
      return;
    }

    setIsLoading(true); setSelectedFolderId(null); setPage(1); setFilter("all");
    setSearchInput(""); setSearchTerm(""); setErrorMessage(""); closeForm();
  };
  const changeViewMode = (mode: ViewMode) => {
    if (mode === viewMode) return;

    setIsLoading(true); setViewMode(mode); setPage(1); setPageSize(mode === "grid" ? 24 : 25);
    localStorage.setItem("rodalltv_media_view", mode);
  };

  const handleUpload = async (request: UploadMediaRequest) => {
    setIsSubmitting(true); setErrorMessage("");
    try { await mediaService.upload(request); setPage(1); refreshMedia(); closeForm(); }
    catch (error) { setErrorMessage(getErrorMessage(error)); }
    finally { setIsSubmitting(false); }
  };

  const handleFolderSubmit = async (request: CreateMediaFolderRequest | UpdateMediaFolderRequest) => {
    setIsSubmitting(true); setErrorMessage("");
    try {
      if (selectedFolderForEdit) await mediaFoldersService.update(selectedFolderForEdit.id, request as UpdateMediaFolderRequest);
      else await mediaFoldersService.create(request as CreateMediaFolderRequest);
      await loadFolders(); closeForm();
    } catch (error) { setErrorMessage(getErrorMessage(error)); }
    finally { setIsSubmitting(false); }
  };

  const handleRemoveFolder = async (folder: MediaFolder) => {
    if (!window.confirm(`¿Deseas eliminar la carpeta "${folder.name}"? Sus archivos se moverán a la raíz.`)) return;
    setErrorMessage("");
    try { await mediaFoldersService.remove(folder.id); await loadFolders(); refreshMedia(); }
    catch (error) { setErrorMessage(getErrorMessage(error)); }
  };

  const handleDeactivate = async (item: MediaItem) => {
    if (!window.confirm(`¿Deseas desactivar "${item.originalFileName}"?`)) return;
    setErrorMessage("");
    try { await mediaService.deactivate(item.id); refreshMedia(); }
    catch (error) { setErrorMessage(getErrorMessage(error)); }
  };

  const handleDownload = async (item: MediaItem) => {
    setErrorMessage("");
    try { await mediaService.download(item); }
    catch (error) { setErrorMessage(getErrorMessage(error)); }
  };

  const handleRename = async (name: string) => {
    if (!renameItem) return;
    setIsSubmitting(true); setErrorMessage("");
    try {
      await mediaService.update(renameItem.id, { originalFileName: name, mediaFolderId: renameItem.mediaFolderId });
      setRenameItem(null); refreshMedia();
    } catch (error) { setErrorMessage(getErrorMessage(error)); }
    finally { setIsSubmitting(false); }
  };

  const handleMoveToRoot = async (item: MediaItem) => {
    if (!item.mediaFolderId || !window.confirm(`¿Mover "${item.originalFileName}" a la raíz?`)) return;
    setErrorMessage("");
    try {
      await mediaService.update(item.id, { originalFileName: item.originalFileName, mediaFolderId: null });
      refreshMedia();
    } catch (error) { setErrorMessage(getErrorMessage(error)); }
  };

  const openContextMenu = (item: MediaItem, left: number, top: number) => setContextMenu({
    item,
    left: Math.max(8, Math.min(left, window.innerWidth - 220)),
    top: Math.max(8, Math.min(top, window.innerHeight - 250)),
  });

  const showsFolders = selectedFolderId === null;
  const hasItems = media.length > 0 || (showsFolders && folders.length > 0);

  return (
    <section>
      <div className="page-heading">
        <div><p className="page-heading__eyebrow">Biblioteca multimedia</p><h2>{selectedFolder?.name ?? "Archivos y carpetas"}</h2><p>{selectedFolder ? "Contenido guardado en esta carpeta." : "Organiza y administra el contenido multimedia."}</p></div>
        <div className="page-heading__actions">
          <button type="button" className="button button--secondary" onClick={refreshMedia} disabled={isLoading}><RefreshCw size={18} />Actualizar</button>
          {canManageMedia && showsFolders && <button type="button" className="button button--secondary" onClick={() => { setSelectedFolderForEdit(null); setFormMode("folder"); }}><Plus size={18} />Nueva carpeta</button>}
          {canManageMedia && <button type="button" className="button button--primary" onClick={() => { setFormMode("upload"); setSelectedFolderForEdit(null); }}><Upload size={18} />Subir archivo</button>}
        </div>
      </div>

      {errorMessage && <div className="alert alert--error" role="alert">{errorMessage}</div>}
      {canManageMedia && formMode === "upload" && <section className="panel"><div className="panel__heading"><h3>Subir contenido multimedia</h3><p>Elige el archivo y confirma la carpeta de destino.</p></div><MediaUploadForm key={selectedFolderId ?? "root-upload"} folders={folders} initialFolderId={selectedFolderId} isSubmitting={isSubmitting} onSubmit={handleUpload} onCancel={closeForm} /></section>}
      {canManageMedia && formMode === "folder" && <section className="panel"><div className="panel__heading"><h3>{selectedFolderForEdit ? "Editar carpeta" : "Crear carpeta"}</h3><p>Define un nombre y una descripción opcional.</p></div><MediaFolderForm key={selectedFolderForEdit?.id ?? "new-folder"} folder={selectedFolderForEdit} isSubmitting={isSubmitting} onSubmit={handleFolderSubmit} onCancel={closeForm} /></section>}

      <nav className="file-manager__breadcrumbs" aria-label="Ruta actual">
        <button type="button" onClick={goToRoot} className={selectedFolder ? "file-manager__breadcrumb" : "file-manager__breadcrumb file-manager__breadcrumb--current"} aria-current={selectedFolder ? undefined : "page"}><Home size={16} />Raíz</button>
        {selectedFolder && <><ChevronRight size={16} /><span className="file-manager__breadcrumb file-manager__breadcrumb--current" aria-current="page"><Folder size={16} />{selectedFolder.name}</span></>}
      </nav>

      <section className="panel media-library-panel">
        <div className="media-toolbar">
          <label className="media-search"><Search size={18} /><span className="sr-only">Buscar archivos</span><input type="search" value={searchInput} placeholder="Buscar archivos..." onChange={(event) => setSearchInput(event.target.value)} /></label>
          <div className="media-toolbar__controls">
            <label className="media-filter"><span>Mostrar</span><select value={filter} onChange={(event) => { setIsLoading(true); setFilter(event.target.value as MediaFilter); setPage(1); }}><option value="all">Todos</option><option value="image">Imágenes</option><option value="video">Videos</option></select></label>
            <label className="media-filter"><span>Ordenar</span><select value={sort} onChange={(event) => { setIsLoading(true); setSort(event.target.value as MediaSort); setPage(1); }}><option value="recent">Más recientes</option><option value="nameAsc">Nombre A–Z</option><option value="nameDesc">Nombre Z–A</option></select></label>
            <div className="media-view-toggle" role="group" aria-label="Tipo de vista">
              <button type="button" className={viewMode === "grid" ? "is-active" : ""} aria-label="Vista de cuadrícula" aria-pressed={viewMode === "grid"} onClick={() => changeViewMode("grid")}><Grid2X2 size={18} /></button>
              <button type="button" className={viewMode === "list" ? "is-active" : ""} aria-label="Vista de lista" aria-pressed={viewMode === "list"} onClick={() => changeViewMode("list")}><ListIcon size={19} /></button>
            </div>
          </div>
        </div>

        {isLoading ? <LoadingState message="Cargando contenido..." /> : !hasItems ? (
          <EmptyState title={searchTerm ? "No encontramos archivos" : selectedFolder ? "Esta carpeta está vacía" : "No hay contenido"} description={searchTerm ? "Prueba con otro nombre o cambia los filtros." : selectedFolder ? "Sube un archivo para agregarlo a esta carpeta." : "Crea una carpeta o sube un archivo para comenzar."} />
        ) : <>
          <div className={`file-manager file-manager--${viewMode}`}>
            {showsFolders && folders.map((folder) => <article className="file-manager-item file-manager-folder" key={folder.id}>
              <button type="button" className="file-manager-folder__open" onClick={() => openFolder(folder)} aria-label={`Abrir carpeta ${folder.name}`}><Folder size={viewMode === "grid" ? 46 : 30} /><strong title={folder.name}>{folder.name}</strong>{viewMode === "list" && <span>Carpeta</span>}</button>
              {canManageMedia && <div className="file-manager-folder__actions"><button type="button" className="icon-button" title="Editar carpeta" aria-label={`Editar carpeta ${folder.name}`} onClick={() => { setSelectedFolderForEdit(folder); setFormMode("folder"); }}><Pencil size={17} /></button><button type="button" className="icon-button icon-button--danger" title="Eliminar carpeta" aria-label={`Eliminar carpeta ${folder.name}`} onClick={() => void handleRemoveFolder(folder)}><Trash2 size={17} /></button></div>}
            </article>)}

            {media.map((item) => <article className="file-manager-item file-manager-media" key={item.id} onContextMenu={(event) => { event.preventDefault(); openContextMenu(item, event.clientX, event.clientY); }}>
              <button type="button" className="file-manager-media__open" onClick={() => setPreviewItem(item)} aria-label={`Abrir vista previa de ${item.originalFileName}`}>
                <span className="file-manager-media__preview"><MediaThumbnail item={item} /></span>
                <span className="file-manager-media__name">{item.mediaType === "Image" ? <FileImage size={16} /> : <FileVideo size={16} />}<strong title={item.originalFileName}>{item.originalFileName}</strong></span>
                {viewMode === "list" && <><span className="file-manager-media__meta">{item.mediaType === "Image" ? "Imagen" : "Video"}</span><span className="file-manager-media__meta">{formatFileSize(item.fileSizeBytes)}</span><span className="file-manager-media__meta">{formatDateTime(item.uploadedAt)}</span></>}
              </button>
              <button type="button" className="file-manager-media__more icon-button" aria-label={`Acciones para ${item.originalFileName}`} title="Más acciones" onClick={(event) => { event.stopPropagation(); const rect = event.currentTarget.getBoundingClientRect(); openContextMenu(item, rect.right - 210, rect.bottom + 6); }}><MoreVertical size={18} /></button>
            </article>)}
          </div>

          {totalItems > 0 && <footer className="media-pagination"><span>{totalItems} archivo{totalItems === 1 ? "" : "s"}</span><div><label>Por página <select value={pageSize} onChange={(event) => { setIsLoading(true); setPageSize(Number(event.target.value)); setPage(1); }}><option value={viewMode === "grid" ? 24 : 25}>{viewMode === "grid" ? 24 : 25}</option><option value={50}>50</option><option value={100}>100</option></select></label><button type="button" className="icon-button" aria-label="Página anterior" disabled={page <= 1} onClick={() => { setIsLoading(true); setPage((value) => value - 1); }}><ChevronLeft size={18} /></button><span>Página {page} de {Math.max(totalPages, 1)}</span><button type="button" className="icon-button" aria-label="Página siguiente" disabled={page >= totalPages} onClick={() => { setIsLoading(true); setPage((value) => value + 1); }}><ChevronRight size={18} /></button></div></footer>}
        </>}
      </section>

      {contextMenu && <div className="media-context-menu" role="menu" style={{ left: contextMenu.left, top: contextMenu.top }} onClick={(event) => event.stopPropagation()}>
        <button type="button" role="menuitem" onClick={() => { setPreviewItem(contextMenu.item); setContextMenu(null); }}><FileImage size={17} />Vista previa</button>
        <button type="button" role="menuitem" onClick={() => { setDetailsItem(contextMenu.item); setContextMenu(null); }}><Info size={17} />Detalles</button>
        <button type="button" role="menuitem" onClick={() => { void handleDownload(contextMenu.item); setContextMenu(null); }}><Download size={17} />Descargar</button>
        {canManageMedia && <button type="button" role="menuitem" onClick={() => { setRenameItem(contextMenu.item); setContextMenu(null); }}><Pencil size={17} />Renombrar</button>}
        {canManageMedia && contextMenu.item.mediaFolderId && <button type="button" role="menuitem" onClick={() => { void handleMoveToRoot(contextMenu.item); setContextMenu(null); }}><MoveUp size={17} />Mover a la raíz</button>}
        {canManageMedia && <button type="button" role="menuitem" className="is-danger" onClick={() => { void handleDeactivate(contextMenu.item); setContextMenu(null); }}><Trash2 size={17} />Eliminar</button>}
      </div>}

      {previewItem && <MediaPreviewModal item={previewItem} onClose={closePreview} onDownload={handleDownload} />}
      {detailsItem && <MediaDetailsModal item={detailsItem} onClose={() => setDetailsItem(null)} />}
      {renameItem && <MediaRenameModal item={renameItem} isSubmitting={isSubmitting} onClose={() => setRenameItem(null)} onSubmit={handleRename} />}
    </section>
  );
}
