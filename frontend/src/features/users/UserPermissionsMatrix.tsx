import { LockKeyhole } from "lucide-react";
import { PERMISSIONS } from "../auth/permission.constants";

interface PermissionGroup {
  label: string;
  view: string | null;
  manage: string | null;
}

const PERMISSION_GROUPS: PermissionGroup[] = [
  { label: "Resumen", view: PERMISSIONS.DASHBOARD_VIEW, manage: null },
  { label: "Dispositivos", view: PERMISSIONS.DEVICES_VIEW, manage: PERMISSIONS.DEVICES_MANAGE },
  { label: "Archivos", view: PERMISSIONS.MEDIA_VIEW, manage: PERMISSIONS.MEDIA_MANAGE },
  { label: "Playlists", view: PERMISSIONS.PLAYLISTS_VIEW, manage: PERMISSIONS.PLAYLISTS_MANAGE },
  { label: "Asignaciones", view: PERMISSIONS.ASSIGNMENTS_VIEW, manage: PERMISSIONS.ASSIGNMENTS_MANAGE },
  { label: "Referencias", view: PERMISSIONS.REFERENCES_VIEW, manage: PERMISSIONS.REFERENCES_MANAGE },
  { label: "Sincronizaciones", view: PERMISSIONS.SYNC_LOGS_VIEW, manage: null },
  { label: "Usuarios", view: null, manage: PERMISSIONS.USERS_MANAGE },
];

interface UserPermissionsMatrixProps {
  permissions: string[];
  disabled?: boolean;
  onChange: (permissions: string[]) => void;
}

export function UserPermissionsMatrix({
  permissions,
  disabled = false,
  onChange,
}: UserPermissionsMatrixProps) {
  const updatePermission = (
    permission: string,
    checked: boolean,
    relatedPermission: string | null,
    kind: "view" | "manage",
  ) => {
    const next = new Set(permissions);

    if (checked) next.add(permission);
    else next.delete(permission);

    if (kind === "manage" && checked && relatedPermission) {
      next.add(relatedPermission);
    }

    if (kind === "view" && !checked && relatedPermission) {
      next.delete(relatedPermission);
    }

    onChange([...next]);
  };

  return (
    <fieldset className="users-permissions" disabled={disabled}>
      <legend>Permisos</legend>
      {disabled && (
        <p className="users-permissions__notice">
          <LockKeyhole size={17} aria-hidden="true" />
          Los administradores tienen acceso completo.
        </p>
      )}
      <div className="table-wrapper">
        <table className="users-permissions__table">
          <thead>
            <tr><th>Módulo</th><th>Ver</th><th>Administrar</th></tr>
          </thead>
          <tbody>
            {PERMISSION_GROUPS.map((group) => (
              <tr key={group.label}>
                <th scope="row">{group.label}</th>
                <td>
                  {group.view ? (
                    <input
                      type="checkbox"
                      aria-label={`Ver ${group.label}`}
                      checked={permissions.includes(group.view)}
                      onChange={(event) => updatePermission(
                        group.view!, event.target.checked, group.manage, "view",
                      )}
                    />
                  ) : <span aria-label="No aplica">—</span>}
                </td>
                <td>
                  {group.manage ? (
                    <input
                      type="checkbox"
                      aria-label={`Administrar ${group.label}`}
                      checked={permissions.includes(group.manage)}
                      onChange={(event) => updatePermission(
                        group.manage!, event.target.checked, group.view, "manage",
                      )}
                    />
                  ) : <span aria-label="No aplica">—</span>}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </fieldset>
  );
}
