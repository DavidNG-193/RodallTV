import axios from "axios";
import { Eye, EyeOff, KeyRound, X } from "lucide-react";
import { useEffect, useState, type FormEvent } from "react";
import { usersService } from "./users.service";
import type { UserListItem } from "./users.types";

interface ResetPasswordModalProps {
  user: UserListItem;
  onClose: () => void;
  onReset: (message: string, disclosedPassword: string, userName: string) => void;
}

export function ResetPasswordModal({ user, onClose, onReset }: ResetPasswordModalProps) {
  const [password, setPassword] = useState("");
  const [confirmation, setConfirmation] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmation, setShowConfirmation] = useState(false);
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

    if (password.length < 8) {
      setErrorMessage("La contraseña debe tener al menos 8 caracteres.");
      return;
    }
    if (password !== confirmation) {
      setErrorMessage("Las contraseñas no coinciden.");
      return;
    }

    setIsSubmitting(true);
    try {
      await usersService.resetUserPassword(user.id, password);
      const disclosedPassword = password;
      setPassword("");
      setConfirmation("");
      onReset(
        "Contraseña actualizada correctamente.",
        disclosedPassword,
        `${user.firstName} ${user.lastName}`,
      );
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
            <h3 id="reset-password-title">Definir nueva contraseña</h3>
            <p>{user.firstName} {user.lastName} · {user.email}</p>
          </div>
          <button type="button" className="icon-button" aria-label="Cerrar formulario" disabled={isSubmitting} onClick={onClose}>
            <X size={19} aria-hidden="true" />
          </button>
        </header>

        <form className="users-form" onSubmit={handleSubmit}>
          <div className="form-field">
            <label htmlFor="reset-password">Nueva contraseña</label>
            <div className="users-password-input">
              <input id="reset-password" type={showPassword ? "text" : "password"} value={password} onChange={(event) => setPassword(event.target.value)} minLength={8} maxLength={100} autoComplete="new-password" autoFocus required />
              <button type="button" className="users-password-input__toggle users-password-input__toggle--icon" aria-label={showPassword ? "Ocultar contraseña" : "Ver contraseña"} onClick={() => setShowPassword((current) => !current)}>
                {showPassword ? <EyeOff size={19} aria-hidden="true" /> : <Eye size={19} aria-hidden="true" />}
              </button>
            </div>
          </div>
          <div className="form-field">
            <label htmlFor="reset-password-confirmation">Confirmar nueva contraseña</label>
            <div className="users-password-input">
              <input id="reset-password-confirmation" type={showConfirmation ? "text" : "password"} value={confirmation} onChange={(event) => setConfirmation(event.target.value)} minLength={8} maxLength={100} autoComplete="new-password" required />
              <button type="button" className="users-password-input__toggle users-password-input__toggle--icon" aria-label={showConfirmation ? "Ocultar confirmación" : "Ver confirmación"} onClick={() => setShowConfirmation((current) => !current)}>
                {showConfirmation ? <EyeOff size={19} aria-hidden="true" /> : <Eye size={19} aria-hidden="true" />}
              </button>
            </div>
          </div>

          {errorMessage && <div className="alert alert--error" role="alert">{errorMessage}</div>}

          <div className="users-modal__actions">
            <button type="button" className="button button--secondary" disabled={isSubmitting} onClick={onClose}>Cancelar</button>
            <button type="submit" className="button button--primary" disabled={isSubmitting}>
              <KeyRound size={18} aria-hidden="true" />
              {isSubmitting ? "Actualizando..." : "Guardar contraseña"}
            </button>
          </div>
        </form>
      </section>
    </div>
  );
}
