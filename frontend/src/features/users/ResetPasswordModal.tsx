import axios from "axios";
import { KeyRound, X } from "lucide-react";
import { useEffect, useState, type FormEvent } from "react";
import { usersService } from "./users.service";
import type { UserListItem } from "./users.types";

interface ResetPasswordModalProps {
  user: UserListItem;
  onClose: () => void;
  onReset: (message: string) => void;
}

export function ResetPasswordModal({ user, onClose, onReset }: ResetPasswordModalProps) {
  const [temporaryPassword, setTemporaryPassword] = useState("");
  const [confirmation, setConfirmation] = useState("");
  const [errorMessage, setErrorMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape" && !isSubmitting) onClose();
    };
    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [isSubmitting, onClose]);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setErrorMessage("");

    if (temporaryPassword.length < 8) {
      setErrorMessage("La contraseña temporal debe tener al menos 8 caracteres.");
      return;
    }
    if (temporaryPassword !== confirmation) {
      setErrorMessage("Las contraseñas temporales no coinciden.");
      return;
    }

    setIsSubmitting(true);
    try {
      await usersService.resetUserPassword(user.id, temporaryPassword);
      setTemporaryPassword("");
      setConfirmation("");
      onReset("Contraseña restablecida. El usuario deberá cambiarla en su próximo acceso.");
    } catch (error) {
      const backendMessage = axios.isAxiosError(error) ? error.response?.data?.message : null;
      setErrorMessage(
        typeof backendMessage === "string" && backendMessage.trim()
          ? backendMessage
          : "No fue posible restablecer la contraseña.",
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="users-modal-backdrop">
      <section className="users-modal" role="dialog" aria-modal="true" aria-labelledby="reset-password-title">
        <header className="users-modal__header">
          <div>
            <span className="users-modal__eyebrow">Seguridad de la cuenta</span>
            <h3 id="reset-password-title">Restablecer contraseña</h3>
            <p>{user.firstName} {user.lastName} · {user.email}</p>
          </div>
          <button type="button" className="icon-button" aria-label="Cerrar formulario" disabled={isSubmitting} onClick={onClose}>
            <X size={19} aria-hidden="true" />
          </button>
        </header>

        <form className="users-form" onSubmit={handleSubmit}>
          <div className="form-field">
            <label htmlFor="reset-temporary-password">Contraseña temporal</label>
            <input id="reset-temporary-password" type="password" value={temporaryPassword} onChange={(event) => setTemporaryPassword(event.target.value)} minLength={8} maxLength={100} autoComplete="new-password" autoFocus required />
          </div>
          <div className="form-field">
            <label htmlFor="reset-password-confirmation">Confirmar contraseña temporal</label>
            <input id="reset-password-confirmation" type="password" value={confirmation} onChange={(event) => setConfirmation(event.target.value)} minLength={8} maxLength={100} autoComplete="new-password" required />
          </div>

          {errorMessage && <div className="alert alert--error" role="alert">{errorMessage}</div>}

          <div className="users-modal__actions">
            <button type="button" className="button button--secondary" disabled={isSubmitting} onClick={onClose}>Cancelar</button>
            <button type="submit" className="button button--primary" disabled={isSubmitting}>
              <KeyRound size={18} aria-hidden="true" />
              {isSubmitting ? "Restableciendo..." : "Restablecer contraseña"}
            </button>
          </div>
        </form>
      </section>
    </div>
  );
}
