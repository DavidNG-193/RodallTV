import axios from "axios";
import {
  KeyRound,
  Pencil,
  Plus,
  Power,
  RefreshCw,
  RotateCcw,
  Users,
} from "lucide-react";
import { useCallback, useEffect, useMemo, useState } from "react";
import { EmptyState } from "../../components/common/EmptyState";
import { LoadingState } from "../../components/common/LoadingState";
import { ResetPasswordModal } from "./ResetPasswordModal";
import { PasswordDisclosureModal } from "./PasswordDisclosureModal";
import { UserFormModal } from "./UserFormModal";
import { USER_ROLES } from "./users.constants";
import { usersService } from "./users.service";
import type { UserDetail, UserListItem } from "./users.types";
import "./users.css";

type FormModalState =
  | { mode: "create" }
  | { mode: "edit"; user: UserDetail }
  | null;

interface DisclosedCredential {
  userName: string;
  password: string;
}

type UserListFilter = "active" | "inactive" | "all";

function formatDateTime(value: string | null): string {
  if (!value) return "Nunca";
  return new Intl.DateTimeFormat("es-MX", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

function getUserError(error: unknown, fallback: string): string {
  if (!axios.isAxiosError(error)) return fallback;
  const message = error.response?.data?.message;
  if (typeof message === "string" && message.trim()) return message;
  if (error.response?.status === 404) return "El usuario ya no existe.";
  return fallback;
}

export function UsersPage() {
  const [users, setUsers] = useState<UserListItem[]>([]);
  const [formModal, setFormModal] = useState<FormModalState>(null);
  const [resetUser, setResetUser] = useState<UserListItem | null>(null);
  const [disclosedCredential, setDisclosedCredential] =
    useState<DisclosedCredential | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [changingStatusId, setChangingStatusId] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const [filter, setFilter] = useState<UserListFilter>("active");

  const visibleUsers = useMemo(() => {
    if (filter === "all") return users;
    const shouldBeActive = filter === "active";
    return users.filter((user) => user.isActive === shouldBeActive);
  }, [filter, users]);

  const loadUsers = useCallback(async (showLoading = true) => {
    if (showLoading) setIsLoading(true);
    setErrorMessage("");

    try {
      setUsers(await usersService.getUsers());
    } catch (error) {
      setErrorMessage(getUserError(error, "No fue posible cargar los usuarios."));
    } finally {
      if (showLoading) setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    let isCancelled = false;

    void usersService.getUsers()
      .then((data) => {
        if (!isCancelled) setUsers(data);
      })
      .catch((error: unknown) => {
        if (!isCancelled) {
          setErrorMessage(getUserError(error, "No fue posible cargar los usuarios."));
        }
      })
      .finally(() => {
        if (!isCancelled) setIsLoading(false);
      });

    return () => {
      isCancelled = true;
    };
  }, []);

  const openEditModal = async (user: UserListItem) => {
    setEditingId(user.id);
    setErrorMessage("");
    setSuccessMessage("");
    try {
      const detail = await usersService.getUserById(user.id);
      setFormModal({ mode: "edit", user: detail });
    } catch (error) {
      setErrorMessage(getUserError(error, "No fue posible consultar el usuario."));
    } finally {
      setEditingId(null);
    }
  };

  const handleStatusChange = async (user: UserListItem) => {
    const nextStatus = !user.isActive;

    if (!nextStatus) {
      const confirmed = window.confirm(
        "¿Deseas desactivar este usuario? Sus sesiones actuales dejarán de ser válidas.",
      );
      if (!confirmed) return;
    }

    setChangingStatusId(user.id);
    setErrorMessage("");
    setSuccessMessage("");
    try {
      await usersService.updateUserStatus(user.id, nextStatus);
      setSuccessMessage(
        nextStatus
          ? "Usuario activado correctamente."
          : "Usuario desactivado correctamente.",
      );
      await loadUsers(false);
    } catch (error) {
      setErrorMessage(getUserError(error, "No fue posible cambiar el estado del usuario."));
    } finally {
      setChangingStatusId(null);
    }
  };

  const handleSaved = (
    message: string,
    disclosedPassword?: string,
    userName?: string,
  ) => {
    setFormModal(null);
    setSuccessMessage(message);
    if (disclosedPassword && userName) {
      setDisclosedCredential({ password: disclosedPassword, userName });
    }
    void loadUsers(false);
  };

  const handlePasswordReset = (
    message: string,
    disclosedPassword: string,
    userName: string,
  ) => {
    setResetUser(null);
    setSuccessMessage(message);
    setDisclosedCredential({ password: disclosedPassword, userName });
    void loadUsers(false);
  };

  return (
    <section className="users-page">
      <div className="page-heading">
        <div>
          <p className="page-heading__eyebrow">Administración</p>
          <h2>Usuarios</h2>
          <p>Administra las cuentas y permisos de acceso al sistema.</p>
        </div>

        <div className="page-heading__actions">
          <div className="device-filter">
            <label htmlFor="user-filter">Mostrar</label>
            <select
              id="user-filter"
              value={filter}
              onChange={(event) => setFilter(event.target.value as UserListFilter)}
            >
              <option value="active">Activos</option>
              <option value="inactive">Inactivos</option>
              <option value="all">Todos</option>
            </select>
          </div>
          <button type="button" className="button button--secondary" disabled={isLoading} onClick={() => void loadUsers()}>
            <RefreshCw size={18} aria-hidden="true" />
            Actualizar
          </button>
          <button
            type="button"
            className="button button--primary"
            onClick={() => {
              setErrorMessage("");
              setSuccessMessage("");
              setFormModal({ mode: "create" });
            }}
          >
            <Plus size={18} aria-hidden="true" />
            Nuevo usuario
          </button>
        </div>
      </div>

      {errorMessage && (
        <div className="alert alert--error users-page__alert" role="alert">
          <span>{errorMessage}</span>
          {users.length === 0 && (
            <button type="button" className="button button--secondary" onClick={() => void loadUsers()}>Reintentar</button>
          )}
        </div>
      )}

      {successMessage && <div className="alert alert--success" role="status">{successMessage}</div>}

      <section className="panel users-list-card">
        <div className="panel__heading users-list-card__heading">
          <div>
            <h3>Cuentas del sistema</h3>
            <p>
              {visibleUsers.length} usuario{visibleUsers.length === 1 ? "" : "s"}
              {filter === "active" ? " activo" : filter === "inactive" ? " inactivo" : " registrado"}
              {visibleUsers.length === 1 ? "" : "s"}
            </p>
          </div>
          <span className="users-list-card__icon"><Users size={22} aria-hidden="true" /></span>
        </div>

        {isLoading ? (
          <LoadingState message="Cargando usuarios..." />
        ) : visibleUsers.length === 0 ? (
          <EmptyState
            title={filter === "active" ? "No hay usuarios activos" : filter === "inactive" ? "No hay usuarios inactivos" : "No hay usuarios para mostrar"}
            description={filter === "all" ? "Crea una cuenta para comenzar a administrar accesos." : "Selecciona otro filtro para consultar las demás cuentas."}
          />
        ) : (
          <div className="table-wrapper">
            <table className="data-table users-table">
              <thead>
                <tr>
                  <th>Nombre</th>
                  <th>Correo</th>
                  <th>Rol</th>
                  <th>Estado</th>
                  <th>Último acceso</th>
                  <th>Contraseña</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {visibleUsers.map((user) => (
                  <tr key={user.id}>
                    <td><strong>{user.firstName} {user.lastName}</strong><small>Creado {formatDateTime(user.createdAt)}</small></td>
                    <td>{user.email}</td>
                    <td><span className={`users-role users-role--${user.role.toLowerCase()}`}>{user.role === USER_ROLES.ADMINISTRATOR ? "Administrador" : "Usuario"}</span></td>
                    <td><span className={`record-status record-status--${user.isActive ? "active" : "inactive"}`}>{user.isActive ? "Activo" : "Inactivo"}</span></td>
                    <td>{formatDateTime(user.lastLoginAt)}</td>
                    <td><span className="users-password-status users-password-status--updated">Configurada</span></td>
                    <td>
                      <div className="table-actions users-table__actions">
                        <button type="button" className="icon-button" aria-label={`Editar a ${user.firstName} ${user.lastName}`} title="Editar usuario" disabled={editingId === user.id} onClick={() => void openEditModal(user)}>
                          <Pencil size={17} aria-hidden="true" />
                        </button>
                        <button type="button" className={`icon-button${user.isActive ? " icon-button--danger" : ""}`} aria-label={`${user.isActive ? "Desactivar" : "Activar"} a ${user.firstName} ${user.lastName}`} title={user.isActive ? "Desactivar usuario" : "Activar usuario"} disabled={changingStatusId === user.id} onClick={() => void handleStatusChange(user)}>
                          {user.isActive ? <Power size={17} aria-hidden="true" /> : <RotateCcw size={17} aria-hidden="true" />}
                        </button>
                        <button type="button" className="icon-button" aria-label={`Definir nueva contraseña para ${user.firstName} ${user.lastName}`} title="Definir nueva contraseña" onClick={() => {
                          setErrorMessage("");
                          setSuccessMessage("");
                          setResetUser(user);
                        }}>
                          <KeyRound size={17} aria-hidden="true" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      {formModal?.mode === "create" && (
        <UserFormModal key="create-user" mode="create" onClose={() => setFormModal(null)} onSaved={handleSaved} />
      )}
      {formModal?.mode === "edit" && (
        <UserFormModal key={formModal.user.id} mode="edit" user={formModal.user} onClose={() => setFormModal(null)} onSaved={handleSaved} />
      )}
      {resetUser && (
        <ResetPasswordModal key={resetUser.id} user={resetUser} onClose={() => setResetUser(null)} onReset={handlePasswordReset} />
      )}
      {disclosedCredential && (
        <PasswordDisclosureModal
          userName={disclosedCredential.userName}
          password={disclosedCredential.password}
          onClose={() => setDisclosedCredential(null)}
        />
      )}
    </section>
  );
}
