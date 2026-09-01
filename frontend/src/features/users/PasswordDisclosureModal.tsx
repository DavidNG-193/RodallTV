import { Eye, EyeOff, KeyRound, X } from "lucide-react";
import { useEffect, useState } from "react";

interface PasswordDisclosureModalProps {
  userName: string;
  password: string;
  onClose: () => void;
}

export function PasswordDisclosureModal({
  userName,
  password,
  onClose,
}: PasswordDisclosureModalProps) {
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") onClose();
    };
    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [onClose]);

  return (
    <div className="users-modal-backdrop">
      <section
        className="users-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="password-disclosure-title"
      >
        <header className="users-modal__header">
          <div>
            <span className="users-modal__eyebrow">Contraseña definida</span>
            <h3 id="password-disclosure-title">Acceso de {userName}</h3>
          </div>
          <button type="button" className="icon-button" aria-label="Cerrar" onClick={onClose}>
            <X size={19} aria-hidden="true" />
          </button>
        </header>

        <div className="users-password-disclosure">
          <p>
            Por seguridad, esta contraseña sólo puede consultarse ahora. Al cerrar
            este aviso no podrá recuperarse; únicamente podrá definirse una nueva.
          </p>
          <div className="form-field">
            <label htmlFor="disclosed-password"><KeyRound size={17} aria-hidden="true" /> Contraseña</label>
            <div className="users-password-input">
              <input
                id="disclosed-password"
                type={isVisible ? "text" : "password"}
                value={password}
                readOnly
              />
              <button
                type="button"
                className="users-password-input__toggle"
                aria-label={isVisible ? "Ocultar contraseña" : "Ver contraseña"}
                onClick={() => setIsVisible((current) => !current)}
              >
                {isVisible ? <EyeOff size={18} aria-hidden="true" /> : <Eye size={18} aria-hidden="true" />}
                {isVisible ? "Ocultar" : "Ver"}
              </button>
            </div>
          </div>
        </div>

        <div className="users-modal__actions">
          <button type="button" className="button button--primary" onClick={onClose}>Entendido</button>
        </div>
      </section>
    </div>
  );
}
