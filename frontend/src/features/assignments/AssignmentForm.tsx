import {
  useState,
  type FormEvent,
} from "react";
import type { Device } from "../devices/devices.types";
import type { Playlist } from "../playlist/playlists.types";
import type {
  CreatePlaylistAssignmentRequest,
} from "./assignments.types";

interface AssignmentFormProps {
  devices: Device[];
  playlists: Playlist[];
  initialDeviceId?: string;
  initialPlaylistId?: string;
  isSubmitting: boolean;
  onSubmit: (
    request: CreatePlaylistAssignmentRequest,
  ) => Promise<void>;
  onCancel: () => void;
}

export function AssignmentForm({
  devices,
  playlists,
  initialDeviceId = "",
  initialPlaylistId = "",
  isSubmitting,
  onSubmit,
  onCancel,
}: AssignmentFormProps) {
  const [deviceId, setDeviceId] =
    useState(initialDeviceId);

  const [playlistId, setPlaylistId] =
    useState(initialPlaylistId);

  const handleSubmit = async (
    event: FormEvent<HTMLFormElement>,
  ) => {
    event.preventDefault();

    await onSubmit({
      deviceId,
      playlistId,
    });
  };

  return (
    <form className="assignment-form" onSubmit={handleSubmit}>
      <div className="form-field">
        <label htmlFor="assignment-device">
          Dispositivo
        </label>

        <select
          id="assignment-device"
          value={deviceId}
          onChange={(event) =>
            setDeviceId(event.target.value)
          }
          required
        >
          <option value="">
            Selecciona un dispositivo
          </option>

          {devices.map((device) => (
            <option key={device.id} value={device.id}>
              {device.name}
            </option>
          ))}
        </select>
      </div>

      <div className="form-field">
        <label htmlFor="assignment-playlist">
          Playlist
        </label>

        <select
          id="assignment-playlist"
          value={playlistId}
          onChange={(event) =>
            setPlaylistId(event.target.value)
          }
          required
        >
          <option value="">
            Selecciona una playlist
          </option>

          {playlists.map((playlist) => (
            <option key={playlist.id} value={playlist.id}>
              {playlist.name} — versión {playlist.version}
            </option>
          ))}
        </select>
      </div>

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
            isSubmitting ||
            !deviceId ||
            !playlistId
          }
        >
          {isSubmitting
            ? "Asignando..."
            : "Guardar asignación"}
        </button>
      </div>
    </form>
  );
}