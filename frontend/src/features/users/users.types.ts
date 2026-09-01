export interface UserListItem {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  isActive: boolean;
  mustChangePassword: boolean;
  createdAt: string;
  lastLoginAt: string | null;
}

export interface UserDetail extends UserListItem {
  permissions: string[];
}

export interface CreateUserPayload {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  role: string;
  permissions: string[];
}

export interface UpdateUserPayload {
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  permissions: string[];
}
