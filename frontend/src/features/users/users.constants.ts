export const USER_ROLES = {
  ADMINISTRATOR: "Administrator",
  USER: "User",
} as const;

export const USER_ROLE_OPTIONS = [
  { value: USER_ROLES.ADMINISTRATOR, label: "Administrador" },
  { value: USER_ROLES.USER, label: "Usuario" },
] as const;
