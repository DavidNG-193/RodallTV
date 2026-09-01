export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthUser {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  mustChangePassword: boolean;
  permissions: string[];
}

export interface LoginResponse extends AuthUser {
  token: string;
  user?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}
