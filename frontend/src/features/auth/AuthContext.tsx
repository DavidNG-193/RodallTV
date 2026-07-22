import { useMemo, useState, type ReactNode } from "react";
import { authService } from "./auth.service";
import type { AuthUser, LoginRequest } from "./auth.types";
import { AuthContext, type AuthContextValue } from "./auth.context";

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<AuthUser | null>(
    authService.getStoredUser(),
  );

  const login = async (credentials: LoginRequest) => {
    const authenticatedUser = await authService.login(credentials);
    setUser(authenticatedUser);
  };

  const logout = () => {
    authService.logout();
    setUser(null);
  };

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isAuthenticated: Boolean(user && authService.getToken()),
      login,
      logout,
    }),
    [user],
  );

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}
