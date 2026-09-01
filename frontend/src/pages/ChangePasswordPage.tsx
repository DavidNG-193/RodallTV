import axios from "axios";
import { Eye, EyeOff, KeyRound, LogOut } from "lucide-react";
import { useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { brandAssets } from "../config/brandAssets";
import { authService } from "../features/auth/auth.service";
import { useAuth } from "../features/auth/useAuth";
import "../styles/login.css";

export function ChangePasswordPage() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmation, setConfirmation] = useState("");
  const [showCurrentPassword, setShowCurrentPassword] = useState(false);
  const [showNewPassword, setShowNewPassword] = useState(false);
  const [showConfirmation, setShowConfirmation] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setErrorMessage("");

    if (newPassword.length < 8) {
      setErrorMessage("La nueva contraseña debe tener al menos 8 caracteres.");
      return;
    }
    if (newPassword !== confirmation) {
      setErrorMessage("La confirmación no coincide con la nueva contraseña.");
      return;
    }
    if (newPassword === currentPassword) {
      setErrorMessage("La nueva contraseña debe ser diferente de la actual.");
      return;
    }

    setIsSubmitting(true);
    try {
      await authService.changePassword({
        currentPassword,
        newPassword,
        confirmPassword: confirmation,
      });
      logout();
      navigate("/login", {
        replace: true,
        state: { message: "Contraseña actualizada. Inicia sesión nuevamente." },
      });
    } catch (error) {
      setErrorMessage(
        axios.isAxiosError(error) && error.response?.status === 400
          ? (error.response.data?.message ?? "No fue posible cambiar la contraseña. Verifica la contraseña actual.")
          : "No fue posible cambiar la contraseña. Inténtalo nuevamente.",
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <main className="login-page" style={{ backgroundImage: `url("${brandAssets.loginBackground}")` }}>
      <div className="login-page__overlay" aria-hidden="true" />
      <section className="login-card change-password-card" aria-labelledby="change-password-title">
        <div className="login-card__brand">
          <img className="login-card__logo" src={brandAssets.mainLogo} alt="Rodall Oseguera" />
          <h1 id="change-password-title">Cambiar contraseña</h1>
          <p>{user?.mustChangePassword ? "Debes actualizar tu contraseña para continuar." : "Actualiza la contraseña de tu cuenta."}</p>
        </div>
        <form className="login-form" onSubmit={handleSubmit}>
          <div className="login-field">
            <label htmlFor="current-password"><KeyRound size={20} aria-hidden="true" />Contraseña actual</label>
            <div className="login-password">
              <input id="current-password" type={showCurrentPassword ? "text" : "password"} value={currentPassword} onChange={(event) => setCurrentPassword(event.target.value)} autoComplete="current-password" required />
              <button type="button" className="login-password__toggle" aria-label={showCurrentPassword ? "Ocultar contraseña actual" : "Ver contraseña actual"} onClick={() => setShowCurrentPassword((current) => !current)}>
                {showCurrentPassword ? <EyeOff size={21} aria-hidden="true" /> : <Eye size={21} aria-hidden="true" />}
              </button>
            </div>
          </div>
          <div className="login-field">
            <label htmlFor="new-password"><KeyRound size={20} aria-hidden="true" />Nueva contraseña</label>
            <div className="login-password">
              <input id="new-password" type={showNewPassword ? "text" : "password"} value={newPassword} onChange={(event) => setNewPassword(event.target.value)} autoComplete="new-password" minLength={8} required />
              <button type="button" className="login-password__toggle" aria-label={showNewPassword ? "Ocultar nueva contraseña" : "Ver nueva contraseña"} onClick={() => setShowNewPassword((current) => !current)}>
                {showNewPassword ? <EyeOff size={21} aria-hidden="true" /> : <Eye size={21} aria-hidden="true" />}
              </button>
            </div>
          </div>
          <div className="login-field">
            <label htmlFor="password-confirmation"><KeyRound size={20} aria-hidden="true" />Confirmar nueva contraseña</label>
            <div className="login-password">
              <input id="password-confirmation" type={showConfirmation ? "text" : "password"} value={confirmation} onChange={(event) => setConfirmation(event.target.value)} autoComplete="new-password" minLength={8} required />
              <button type="button" className="login-password__toggle" aria-label={showConfirmation ? "Ocultar confirmación" : "Ver confirmación"} onClick={() => setShowConfirmation((current) => !current)}>
                {showConfirmation ? <EyeOff size={21} aria-hidden="true" /> : <Eye size={21} aria-hidden="true" />}
              </button>
            </div>
          </div>
          {errorMessage && <div className="login-alert" role="alert">{errorMessage}</div>}
          <button className="login-submit" type="submit" disabled={isSubmitting}>
            <KeyRound size={21} aria-hidden="true" />
            {isSubmitting ? "Actualizando..." : "Actualizar contraseña"}
          </button>
          {!user?.mustChangePassword && (
            <button className="change-password-card__cancel" type="button" onClick={() => navigate(-1)}>
              <LogOut size={18} aria-hidden="true" />Cancelar
            </button>
          )}
        </form>
      </section>
    </main>
  );
}
