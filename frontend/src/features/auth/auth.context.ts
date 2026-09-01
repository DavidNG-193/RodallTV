import { createContext } from "react";
import type { AuthUser, LoginRequest } from "./auth.types";

export interface AuthContextValue {
  user: AuthUser | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: LoginRequest) => Promise<AuthUser>;
  logout: () => void;
  refreshCurrentUser: () => Promise<AuthUser>;
  hasPermission: (permission: string) => boolean;
}

export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined,
);
