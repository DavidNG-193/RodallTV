import axios from "axios";
import {
  Link2,
  Pencil,
  Plus,
  RefreshCw,
  Unlink,
} from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import { EmptyState } from "../components/common/EmptyState";
import { LoadingState } from "../components/common/LoadingState";
import { AssignmentForm } from "../features/assignments/AssignmentForm";
import { assignmentsService } from "../features/assignments/assignments.service";
import type {
  CreatePlaylistAssignmentRequest,
  PlaylistAssignment,
} from "../features/assignments/assignments.types";
import { devicesService } from "../features/devices/devices.service";
import type { Device } from "../features/devices/devices.types";
import { playlistsService } from "../features/playlist/playlists.service";
import type { Playlist } from "../features/playlist/playlists.types";
import { formatDateTime } from "../utils/fileFormatters";

interface AssignmentPageData {
  assignments: PlaylistAssignment[];
  devices: Device[];
  playlists: Playlist[];
}

function getErrorMessage(error: unknown): string {
  if (!axios.isAxiosError(error)) {
    return "Ocurrió un error inesperado.";
  }

  if (error.response?.status === 400) {
    return "Los datos de la asignación no son válidos.";
  }

  if (error.response?.status === 401) {
    return "La sesión expiró. Inicia sesión nuevamente.";
  }

  if (error.response?.status === 404) {
    return "El dispositivo o la playlist no existe o está inactivo.";
  }

  if (error.response?.status === 409) {
    return "La playlist seleccionada ya está asignada al dispositivo.";
  }

  return "No fue posible completar la operación.";
}

async function fetchAssignmentData(): Promise<AssignmentPageData> {
  const [assignments, devices, playlists] = await Promise.all([
    assignmentsService.getAll(true),
    devicesService.getAll("active"),
    playlistsService.getAll(),
  ]);

  return {
    assignments: assignments.filter(
      (assignment) => assignment.isActive,
    ),
    devices,
    playlists,
  };
}

export function AssignmentsPage() {
  const [assignments, setAssignments] =
    useState<PlaylistAssignment[]>([]);
  const [devices, setDevices] = useState<Device[]>([]);
  const [playlists, setPlaylists] = useState<Playlist[]>([]);
  const [selectedAssignment, setSelectedAssignment] =
    useState<PlaylistAssignment | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isFormVisible, setIsFormVisible] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  const applyData = useCallback((data: AssignmentPageData) => {
    setAssignments(data.assignments);
    setDevices(data.devices);
    setPlaylists(data.playlists);
  }, []);

  const loadData = useCallback(async () => {
    setIsLoading(true);
    setErrorMessage("");

    try {
      applyData(await fetchAssignmentData());
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsLoading(false);
    }
  }, [applyData]);

  useEffect(() => {
    let isCancelled = false;

    void fetchAssignmentData()
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
  }, [applyData]);

  const openCreateForm = () => {
    setSelectedAssignment(null);
    setErrorMessage("");
    setIsFormVisible(true);
  };

  const openEditForm = (assignment: PlaylistAssignment) => {
    setSelectedAssignment(assignment);
    setErrorMessage("");
    setIsFormVisible(true);
  };

  const closeForm = () => {
    setSelectedAssignment(null);
    setIsFormVisible(false);
  };

  const handleAssign = async (
    request: CreatePlaylistAssignmentRequest,
  ) => {
    setIsSubmitting(true);
    setErrorMessage("");

    try {
      await assignmentsService.assign(request);
      await loadData();
      closeForm();
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleUnassign = async (
    assignment: PlaylistAssignment,
  ) => {
    const confirmed = window.confirm(
      `¿Deseas desasignar la playlist de "${assignment.deviceName}"?`,
    );

    if (!confirmed) {
      return;
    }

    setErrorMessage("");

    try {
      await assignmentsService.unassign(assignment.deviceId);
      await loadData();
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    }
  };

  return (
    <section>
      <div className="page-heading">
        <div>
          <p className="page-heading__eyebrow">Programación</p>
          <h2>Asignaciones</h2>
          <p>
            Define qué playlist debe reproducir cada dispositivo.
          </p>
        </div>

        <div className="page-heading__actions">
          <button
            type="button"
            className="button button--secondary"
            onClick={() => void loadData()}
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
            Nueva asignación
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
              {selectedAssignment
                ? "Cambiar playlist"
                : "Nueva asignación"}
            </h3>
            <p>
              Una nueva asignación reemplaza la asignación activa del
              dispositivo.
            </p>
          </div>

          <AssignmentForm
            key={
              selectedAssignment
                ? `${selectedAssignment.deviceId}-${selectedAssignment.playlistId}`
                : "new-assignment"
            }
            devices={devices}
            playlists={playlists}
            initialDeviceId={selectedAssignment?.deviceId}
            initialPlaylistId={selectedAssignment?.playlistId}
            isSubmitting={isSubmitting}
            onSubmit={handleAssign}
            onCancel={closeForm}
          />
        </section>
      )}

      <section className="panel">
        {isLoading ? (
          <LoadingState message="Cargando asignaciones..." />
        ) : assignments.length === 0 ? (
          <EmptyState
            title="No hay asignaciones activas"
            description="Asigna una playlist a un dispositivo para comenzar."
          />
        ) : (
          <div className="assignment-grid">
            {assignments.map((assignment) => (
              <article
                className="assignment-card"
                key={assignment.id}
              >
                <div className="assignment-card__icon">
                  <Link2 size={25} aria-hidden="true" />
                </div>

                <div className="assignment-card__content">
                  <h3>{assignment.deviceName}</h3>
                  <p>{assignment.playlistName}</p>
                  <small>
                    Versión {assignment.playlistVersion} · Asignada el{" "}
                    {formatDateTime(assignment.assignedAt)}
                  </small>
                  <small>Por {assignment.assignedByEmail}</small>
                </div>

                <div className="assignment-card__actions">
                  <button
                    type="button"
                    className="button button--secondary"
                    onClick={() => openEditForm(assignment)}
                  >
                    <Pencil size={17} aria-hidden="true" />
                    Cambiar playlist
                  </button>

                  <button
                    type="button"
                    className="icon-button icon-button--danger"
                    title="Desasignar playlist"
                    onClick={() =>
                      void handleUnassign(assignment)
                    }
                  >
                    <Unlink size={17} aria-hidden="true" />
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
