import axios from "axios";

type ReferenceAction = "load" | "lookup" | "create" | "delete" | "refresh";

const fallbackMessages: Record<ReferenceAction, string> = {
  load: "No fue posible cargar las referencias.",
  lookup: "No fue posible buscar la referencia.",
  create: "No fue posible agregar la referencia.",
  delete: "No fue posible eliminar la referencia.",
  refresh: "No fue posible actualizar las referencias.",
};

export function getReferenceErrorMessage(
  error: unknown,
  action: ReferenceAction,
): string {
  if (!axios.isAxiosError(error)) {
    return fallbackMessages[action];
  }

  switch (error.response?.status) {
    case 400:
      return "Escribe un número de referencia válido.";
    case 401:
      return "La sesión expiró. Inicia sesión nuevamente.";
    case 404:
      return action === "lookup" || action === "create"
        ? "La referencia no fue encontrada."
        : "La referencia ya no existe.";
    case 409:
      return "La referencia ya está agregada.";
    case 502:
      return "Saga devolvió una respuesta inválida.";
    case 503:
      return "El servicio de referencias no está disponible temporalmente.";
    default:
      return fallbackMessages[action];
  }
}
