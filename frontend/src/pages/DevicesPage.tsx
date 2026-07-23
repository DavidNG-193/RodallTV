import { Pencil, Plus, Power, RefreshCw } from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import axios from "axios";
import { EmptyState } from "../components/common/EmptyState";
import { LoadingState } from "../components/common/LoadingState";
import { StatusBadge } from "../components/common/StatusBadge";
import { DeviceForm } from "../features/devices/DeviceForm";
import { devicesService } from "../features/devices/devices.service";
import type {
  CreateDeviceRequest,
  CreateDeviceResponse,
  Device,
  UpdateDeviceRequest,
} from "../features/devices/devices.types";

function formatDate(value: string | null): string {
  if (!value) return "Sin registro";

  return new Intl.DateTimeFormat("es-MX", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

function getErrorMessage(error: unknown): string {
  if (!axios.isAxiosError(error)) return "Ocurrió un error inesperado.";
  if (error.response?.status === 409) {
    return "Ya existe un dispositivo con ese nombre.";
  }
  if (error.response?.status === 401) {
    return "La sesión expiró. Inicia sesión nuevamente.";
  }
  return "No fue posible completar la operación.";
}

export function DevicesPage() {
  const [devices, setDevices] = useState<Device[]>([]);
  const [selectedDevice, setSelectedDevice] = useState<Device | null>(null);
  const [createdDevice, setCreatedDevice] =
    useState<CreateDeviceResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isFormVisible, setIsFormVisible] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  const loadDevices = useCallback(async () => {
    try {
      const data = await devicesService.getAll();
      setDevices(data);
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    let isCancelled = false;

    void devicesService
      .getAll()
      .then((data) => {
        if (!isCancelled) setDevices(data);
      })
      .catch((error: unknown) => {
        if (!isCancelled) setErrorMessage(getErrorMessage(error));
      })
      .finally(() => {
        if (!isCancelled) setIsLoading(false);
      });

    return () => {
      isCancelled = true;
    };
  }, []);

  const refreshDevices = () => {
    setIsLoading(true);
    setErrorMessage("");
    void loadDevices();
  };

  const openCreateForm = () => {
    setSelectedDevice(null);
    setCreatedDevice(null);
    setErrorMessage("");
    setIsFormVisible(true);
  };

  const openEditForm = (device: Device) => {
    setSelectedDevice(device);
    setCreatedDevice(null);
    setErrorMessage("");
    setIsFormVisible(true);
  };

  const closeForm = () => {
    setSelectedDevice(null);
    setIsFormVisible(false);
  };

  const handleSubmit = async (
    request: CreateDeviceRequest | UpdateDeviceRequest,
  ) => {
    setIsSubmitting(true);
    setErrorMessage("");

    try {
      if (selectedDevice) {
        await devicesService.update(
          selectedDevice.id,
          request as UpdateDeviceRequest,
        );
      } else {
        const created = await devicesService.create(
          request as CreateDeviceRequest,
        );
        setCreatedDevice(created);
      }

      await loadDevices();

      if (selectedDevice) closeForm();
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDeactivate = async (device: Device) => {
    const confirmed = window.confirm(
      `¿Deseas desactivar el dispositivo "${device.name}"?`,
    );

    if (!confirmed) return;

    try {
      await devicesService.deactivate(device.id);
      await loadDevices();
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    }
  };

  return (
    <section>
      <div className="page-heading">
        <div>
          <p className="page-heading__eyebrow">Administración</p>
          <h2>Dispositivos</h2>
          <p>Registra y consulta las Raspberry Pi conectadas a las pantallas.</p>
        </div>

        <div className="page-heading__actions">
          <button
            type="button"
            className="button button--secondary"
            onClick={refreshDevices}
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
            Nuevo dispositivo
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
          <h3>{selectedDevice ? "Editar dispositivo" : "Registrar dispositivo"}</h3>

          <DeviceForm
            key={selectedDevice?.id ?? "new-device"}
            device={selectedDevice}
            isSubmitting={isSubmitting}
            onSubmit={handleSubmit}
            onCancel={closeForm}
          />

          {createdDevice && (
            <div className="token-panel">
              <strong>Credenciales del nuevo dispositivo</strong>
              <p>Guarda estos datos ahora. El token no se volverá a mostrar.</p>
              <p><b>Device UUID:</b> {createdDevice.deviceUuid}</p>
              <p><b>Token:</b> {createdDevice.accessToken}</p>
            </div>
          )}
        </section>
      )}

      <section className="panel">
        {isLoading ? (
          <LoadingState message="Cargando dispositivos..." />
        ) : devices.length === 0 ? (
          <EmptyState
            title="No hay dispositivos"
            description="Registra la primera Raspberry Pi para comenzar."
          />
        ) : (
          <div className="table-wrapper">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Nombre</th>
                  <th>Ubicación</th>
                  <th>Estado</th>
                  <th>IP</th>
                  <th>Última conexión</th>
                  <th>Última sincronización</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {devices.map((device) => (
                  <tr key={device.id}>
                    <td>
                      <strong>{device.name}</strong>
                      <small>{device.deviceUuid}</small>
                    </td>
                    <td>{device.location ?? "Sin ubicación"}</td>
                    <td><StatusBadge status={device.status} /></td>
                    <td>{device.ipAddress ?? "Sin registro"}</td>
                    <td>{formatDate(device.lastConnectionAt)}</td>
                    <td>{formatDate(device.lastSyncAt)}</td>
                    <td>
                      <div className="table-actions">
                        <button
                          type="button"
                          className="icon-button"
                          title="Editar dispositivo"
                          onClick={() => openEditForm(device)}
                        >
                          <Pencil size={17} aria-hidden="true" />
                        </button>

                        {device.isActive && (
                          <button
                            type="button"
                            className="icon-button icon-button--danger"
                            title="Desactivar dispositivo"
                            onClick={() => void handleDeactivate(device)}
                          >
                            <Power size={17} aria-hidden="true" />
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </section>
  );
}
