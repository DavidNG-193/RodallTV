import { useState, type FormEvent } from "react";
import type {
  CreateDeviceRequest,
  Device,
  UpdateDeviceRequest,
} from "./devices.types";

interface DeviceFormProps {
  device?: Device | null;
  isSubmitting: boolean;
  onSubmit: (
    request: CreateDeviceRequest | UpdateDeviceRequest,
  ) => Promise<void>;
  onCancel: () => void;
}

export function DeviceForm({
  device,
  isSubmitting,
  onSubmit,
  onCancel,
}: DeviceFormProps) {
  const [name, setName] = useState(device?.name ?? "");
  const [location, setLocation] = useState(device?.location ?? "");

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    await onSubmit({
      name: name.trim(),
      location: location.trim() || null,
    });
  };

  return (
    <form className="device-form" onSubmit={handleSubmit}>
      <div className="form-field">
        <label htmlFor="device-name">Nombre</label>
        <input
          id="device-name"
          type="text"
          value={name}
          onChange={(event) => setName(event.target.value)}
          required
        />
      </div>

      <div className="form-field">
        <label htmlFor="device-location">Ubicación</label>
        <input
          id="device-location"
          type="text"
          value={location}
          onChange={(event) => setLocation(event.target.value)}
          placeholder="Ejemplo: Cocina"
        />
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
          disabled={isSubmitting || !name.trim()}
        >
          {isSubmitting
            ? "Guardando..."
            : device
              ? "Guardar cambios"
              : "Registrar dispositivo"}
        </button>
      </div>
    </form>
  );
}
