import axios from "axios";
import { Eye, EyeOff, ShieldCheck, UserPlus, X } from "lucide-react";
import { useEffect, useState, type FormEvent } from "react";
import { PERMISSIONS } from "../auth/permission.constants";
import { useAuth } from "../auth/useAuth";
import { UserPermissionsMatrix } from "./UserPermissionsMatrix";
import { USER_ROLES, USER_ROLE_OPTIONS } from "./users.constants";
import { usersService } from "./users.service";
import type { UserDetail } from "./users.types";

interface UserFormModalProps {
  mode: "create" | "edit";
  user?: UserDetail;
  onClose: () => void;
  onSaved: (message: string, disclosedPassword?: string, userName?: string) => void;
}

const allPermissions = Object.values(PERMISSIONS);

function getSubmitError(error: unknown): string {
  if (!axios.isAxiosError(error)) return "Ocurrió un error inesperado.";

  const message = error.response?.data?.message;
  if (typeof message === "string" && message.trim()) return message;
  if (error.response?.status === 404) return "El usuario ya no existe.";
  if (error.response?.status === 400) return "Revisa los datos capturados.";
  return "No fue posible guardar el usuario.";
}

export function UserFormModal({
  mode,
  user,
  onClose,
  onSaved,
}: UserFormModalProps) {
  const { user: currentUser } = useAuth();
  const [firstName, setFirstName] = useState(user?.firstName ?? "");
  const [lastName, setLastName] = useState(user?.lastName ?? "");
  const [email, setEmail] = useState(user?.email ?? "");
  const [role, setRole] = useState(user?.role ?? USER_ROLES.USER);
  const [permissions, setPermissions] = useState<string[]>(
    user?.role === USER_ROLES.ADMINISTRATOR
      ? allPermissions
      : (user?.permissions ?? []),
  );
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const isAdministrator = role === USER_ROLES.ADMINISTRATOR;
  const isEditingSelf = mode === "edit" && currentUser?.userId === user?.id;

  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape" && !isSubmitting) onClose();
    };
    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [isSubmitting, onClose]);

  const handleRoleChange = (nextRole: string) => {
    setRole(nextRole);
    if (nextRole === USER_ROLES.ADMINISTRATOR) {
      setPermissions(allPermissions);
    } else if (role === USER_ROLES.ADMINISTRATOR) {
      setPermissions(user?.role === USER_ROLES.USER ? user.permissions : []);
    }
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setErrorMessage("");

    const normalizedFirstName = firstName.trim();
    const normalizedLastName = lastName.trim();
    const normalizedEmail = email.trim();

    if (!normalizedFirstName || !normalizedLastName || !normalizedEmail) {
      setErrorMessage("Nombre, apellido y correo son obligatorios.");
      return;
    }
    if (!/^\S+@\S+\.\S+$/.test(normalizedEmail)) {
      setErrorMessage("Captura un correo electrónico válido.");
      return;
    }
    if (!USER_ROLE_OPTIONS.some((option) => option.value === role)) {
      setErrorMessage("Selecciona un rol válido.");
      return;
    }
    if (mode === "create" && password.length < 8) {
      setErrorMessage("La contraseña debe tener al menos 8 caracteres.");
      return;
    }

    setIsSubmitting(true);
    try {
      const selectedPermissions = isAdministrator ? allPermissions : permissions;

      if (mode === "create") {
        await usersService.createUser({
          firstName: normalizedFirstName,
          lastName: normalizedLastName,
          email: normalizedEmail,
          password,
          role,
          permissions: selectedPermissions,
        });
        const disclosedPassword = password;
        setPassword("");
        onSaved(
          "Usuario creado correctamente.",
          disclosedPassword,
          `${normalizedFirstName} ${normalizedLastName}`,
        );
      } else if (user) {
        await usersService.updateUser(user.id, {
          firstName: normalizedFirstName,
          lastName: normalizedLastName,
          email: normalizedEmail,
          role,
          permissions: selectedPermissions,
        });
        onSaved("Usuario actualizado correctamente.");
      }
    } catch (error) {
      setErrorMessage(getSubmitError(error));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="users-modal-backdrop">
      <section
        className="users-modal users-modal--wide"
        role="dialog"
        aria-modal="true"
        aria-labelledby="user-form-title"
      >
        <header className="users-modal__header">
          <div>
            <span className="users-modal__eyebrow">Administración de acceso</span>
            <h3 id="user-form-title">
              {mode === "create" ? "Nuevo usuario" : "Editar usuario"}
            </h3>
          </div>
          <button type="button" className="icon-button" aria-label="Cerrar formulario" disabled={isSubmitting} onClick={onClose}>
            <X size={19} aria-hidden="true" />
          </button>
        </header>

        <form className="users-form" onSubmit={handleSubmit}>
          <div className="users-form__fields">
            <div className="form-field">
              <label htmlFor="user-first-name">Nombre</label>
              <input id="user-first-name" value={firstName} onChange={(event) => setFirstName(event.target.value)} maxLength={100} autoFocus required />
            </div>
            <div className="form-field">
              <label htmlFor="user-last-name">Apellido</label>
              <input id="user-last-name" value={lastName} onChange={(event) => setLastName(event.target.value)} maxLength={100} required />
            </div>
            <div className="form-field users-form__field--wide">
              <label htmlFor="user-email">Correo electrónico</label>
              <input id="user-email" type="email" value={email} onChange={(event) => setEmail(event.target.value)} maxLength={150} autoComplete="off" required />
            </div>
            <div className="form-field">
              <label htmlFor="user-role">Rol</label>
              <select id="user-role" value={role} onChange={(event) => handleRoleChange(event.target.value)} required>
                {USER_ROLE_OPTIONS.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
              </select>
            </div>
            {mode === "create" && (
              <div className="form-field">
                <label htmlFor="user-password">Contraseña</label>
                <div className="users-password-input">
                  <input id="user-password" type={showPassword ? "text" : "password"} value={password} onChange={(event) => setPassword(event.target.value)} minLength={8} maxLength={100} autoComplete="new-password" required />
                  <button type="button" className="users-password-input__toggle users-password-input__toggle--icon" aria-label={showPassword ? "Ocultar contraseña" : "Ver contraseña"} onClick={() => setShowPassword((current) => !current)}>
                    {showPassword ? <EyeOff size={19} aria-hidden="true" /> : <Eye size={19} aria-hidden="true" />}
                  </button>
                </div>
              </div>
            )}
          </div>

          {isEditingSelf && (
            <div className="users-self-notice" role="note">
              <ShieldCheck size={18} aria-hidden="true" />
              Al guardar cambios sensibles en tu propia cuenta es posible que debas iniciar sesión nuevamente.
            </div>
          )}

          <UserPermissionsMatrix
            permissions={isAdministrator ? allPermissions : permissions}
            disabled={isAdministrator}
            onChange={setPermissions}
          />

          {errorMessage && <div className="alert alert--error" role="alert">{errorMessage}</div>}

          <div className="users-modal__actions">
            <button type="button" className="button button--secondary" disabled={isSubmitting} onClick={onClose}>Cancelar</button>
            <button type="submit" className="button button--primary" disabled={isSubmitting}>
              <UserPlus size={18} aria-hidden="true" />
              {isSubmitting ? "Guardando..." : mode === "create" ? "Crear usuario" : "Guardar cambios"}
            </button>
          </div>
        </form>
      </section>
    </div>
  );
}
