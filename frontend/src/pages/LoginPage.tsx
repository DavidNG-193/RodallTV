import {
  Eye,
  EyeOff,
  LockKeyhole,
  LogIn,
  UserRound,
} from "lucide-react";
import {
  useState,
  type FormEvent,
} from "react";
import axios from "axios";
import {
  Navigate,
  useLocation,
  useNavigate,
} from "react-router-dom";
import { brandAssets } from "../config/brandAssets";
import { useAuth } from "../features/auth/useAuth";
import "../styles/login.css";

interface LocationState {
  from?: {
    pathname?: string;
  };
}

export function LoginPage() {
  const { login, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [rememberSession, setRememberSession] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  const handleSubmit = async (
    event: FormEvent<HTMLFormElement>,
  ) => {
    event.preventDefault();
    setErrorMessage("");
    setIsSubmitting(true);

    try {
      await login({
        email: email.trim(),
        password,
      });

      const state = location.state as LocationState | null;
      const destination =
        state?.from?.pathname ?? "/";

      navigate(destination, {
        replace: true,
      });
    } catch (error) {
      if (axios.isAxiosError(error)) {
        if (error.response?.status === 401) {
          setErrorMessage(
            "El correo o la contraseña son incorrectos.",
          );
        } else {
          setErrorMessage(
            "No fue posible iniciar sesión. Verifica la conexión con el servidor.",
          );
        }
      } else {
        setErrorMessage(
          "Ocurrió un error inesperado.",
        );
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <main
      className="login-page"
      style={{
        backgroundImage:
          `url("${brandAssets.loginBackground}")`,
      }}
    >
      <div
        className="login-page__overlay"
        aria-hidden="true"
      />

      <section
        className="login-card"
        aria-labelledby="login-title"
      >
        <div className="login-card__brand">
          <img
            className="login-card__logo"
            src={brandAssets.mainLogo}
            alt="Rodall Oseguera"
          />

          <h1 id="login-title">
            RodallTV
          </h1>

          <p>
            Acceso al sistema de Digital Signage
          </p>
        </div>

        <form
          className="login-form"
          onSubmit={handleSubmit}
        >
          <div className="login-field">
            <label htmlFor="email">
              <UserRound size={20} aria-hidden="true" />
              Correo electrónico
            </label>

            <input
              id="email"
              name="email"
              type="email"
              value={email}
              onChange={(event) =>
                setEmail(event.target.value)
              }
              placeholder="usuario@acero.com"
              autoComplete="email"
              required
            />
          </div>

          <div className="login-field">
            <label htmlFor="password">
              <LockKeyhole size={20} aria-hidden="true" />
              Contraseña
            </label>

            <div className="login-password">
              <input
                id="password"
                name="password"
                type={showPassword ? "text" : "password"}
                value={password}
                onChange={(event) =>
                  setPassword(event.target.value)
                }
                placeholder="Ingresa tu contraseña"
                autoComplete="current-password"
                required
              />

              <button
                type="button"
                className="login-password__toggle"
                onClick={() =>
                  setShowPassword((current) => !current)
                }
                aria-label={
                  showPassword
                    ? "Ocultar contraseña"
                    : "Mostrar contraseña"
                }
              >
                {showPassword ? (
                  <EyeOff size={21} aria-hidden="true" />
                ) : (
                  <Eye size={21} aria-hidden="true" />
                )}
              </button>
            </div>
          </div>

          <div className="login-options">
            <label className="login-checkbox">
              <input
                type="checkbox"
                checked={rememberSession}
                onChange={(event) =>
                  setRememberSession(event.target.checked)
                }
              />
              <span>Recordar sesión</span>
            </label>
          </div>

          {errorMessage && (
            <div
              className="login-alert"
              role="alert"
            >
              {errorMessage}
            </div>
          )}

          <button
            type="submit"
            className="login-submit"
            disabled={isSubmitting}
          >
            <LogIn size={22} aria-hidden="true" />
            {isSubmitting
              ? "Iniciando sesión..."
              : "Iniciar sesión"}
          </button>
        </form>

        <footer className="login-card__footer">
          <p>
            Si no tienes credenciales de acceso,
            contacta al administrador del sistema.
          </p>

          <span>
            © 2026 Agencia Aduanal Rodall Oseguera
          </span>
        </footer>
      </section>
    </main>
  );
}