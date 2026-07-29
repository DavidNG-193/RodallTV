import axios from "axios";
import {
  ChevronDown,
  Power,
  RotateCcw,
} from "lucide-react";
import { useState } from "react";
import { devicesService } from "./devices.service";
import type {
  Device,
  PowerCommandType,
} from "./devices.types";

interface DevicePowerActionsProps {
  device: Device;
}

function getPowerActionError(error: unknown): string {
  if (!axios.isAxiosError(error)) {
    return "No fue posible enviar la orden al dispositivo.";
  }

  const responseData = error.response?.data as
    | { message?: string }
    | undefined;

  return (
    responseData?.message ??
    "No fue posible enviar la orden al dispositivo."
  );
}

export function DevicePowerActions({
  device,
}: DevicePowerActionsProps) {
  const [isPowerMenuOpen, setIsPowerMenuOpen] =
    useState(false);
  const [powerActionLoading, setPowerActionLoading] =
    useState<PowerCommandType | null>(null);
  const [powerActionError, setPowerActionError] =
    useState<string | null>(null);

  const handlePowerCommand = async (
    commandType: PowerCommandType,
  ): Promise<void> => {
    const isRestart = commandType === "Restart";
    const confirmationMessage = isRestart
      ? "La Raspberry dejará de reproducir contenido durante el reinicio y volverá a conectarse automáticamente. ¿Deseas continuar?"
      : "La Raspberry se apagará de forma segura. No podrá encenderse nuevamente desde RodallTV. ¿Deseas continuar?";

    if (!window.confirm(confirmationMessage)) {
      return;
    }

    try {
      setPowerActionError(null);
      setPowerActionLoading(commandType);
      setIsPowerMenuOpen(false);

      const response = await devicesService.sendPowerCommand(
        device.id,
        commandType,
      );

      window.alert(response.message);
    } catch (error) {
      setPowerActionError(getPowerActionError(error));
    } finally {
      setPowerActionLoading(null);
    }
  };

  return (
    <section className="device-power-actions">
      <div className="device-power-actions__header">
        <div>
          <h3>Acciones de energía</h3>
          <p>
            Reinicia o apaga la Raspberry de forma segura.
          </p>
        </div>

        <div className="device-power-menu">
          <button
            type="button"
            className="device-power-menu__trigger"
            onClick={() =>
              setIsPowerMenuOpen((current) => !current)
            }
            disabled={
              powerActionLoading !== null || !device.isActive
            }
            aria-expanded={isPowerMenuOpen}
            aria-haspopup="menu"
          >
            <Power size={18} aria-hidden="true" />

            {powerActionLoading
              ? "Enviando orden..."
              : "Opciones de energía"}

            <ChevronDown size={16} aria-hidden="true" />
          </button>

          {isPowerMenuOpen && (
            <div
              className="device-power-menu__content"
              role="menu"
            >
              <button
                type="button"
                role="menuitem"
                onClick={() =>
                  void handlePowerCommand("Restart")
                }
              >
                <RotateCcw size={17} aria-hidden="true" />

                <span>
                  <strong>Reiniciar dispositivo</strong>
                  <small>
                    La Raspberry volverá a conectarse.
                  </small>
                </span>
              </button>

              <button
                type="button"
                role="menuitem"
                className="device-power-menu__danger"
                onClick={() =>
                  void handlePowerCommand("Shutdown")
                }
              >
                <Power size={17} aria-hidden="true" />

                <span>
                  <strong>Apagar dispositivo</strong>
                  <small>
                    Requerirá energía para volver a iniciar.
                  </small>
                </span>
              </button>
            </div>
          )}
        </div>
      </div>

      {!device.isActive && (
        <p className="device-power-actions__notice">
          Reactiva el dispositivo para enviar una orden.
        </p>
      )}

      {powerActionError && (
        <p
          className="device-power-actions__error"
          role="alert"
        >
          {powerActionError}
        </p>
      )}
    </section>
  );
}
