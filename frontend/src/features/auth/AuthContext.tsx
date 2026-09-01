import {
  useCallback,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { AUTH_UNAUTHORIZED_EVENT } from "../../api/httpClient";
import { authService } from "./auth.service";
import type { AuthUser, LoginRequest } from "./auth.types";
import { AuthContext, type AuthContextValue } from "./auth.context";

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [token, setToken] = useState<string | null>(() => authService.getToken());
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(Boolean(token));

  const clearSession = useCallback(() => {
    authService.logout();
    setToken(null);
    setUser(null);
    setIsLoading(false);
  }, []);

  const refreshCurrentUser = useCallback(async () => {
    const currentUser = await authService.getCurrentUser();
    setToken(authService.getToken());
    setUser(currentUser);
    return currentUser;
  }, []);

  useEffect(() => {
    let isActive = true;
    const storedToken = authService.getToken();

    if (!storedToken) {
      return () => {
        isActive = false;
      };
    }

    authService
      .getCurrentUser()
      .then((currentUser) => {
        if (isActive) setUser(currentUser);
      })
      .catch(() => {
        if (isActive) clearSession();
      })
      .finally(() => {
        if (isActive) setIsLoading(false);
      });

    return () => {
      isActive = false;
    };
  }, [clearSession]);

  useEffect(() => {
    window.addEventListener(AUTH_UNAUTHORIZED_EVENT, clearSession);
    return () => window.removeEventListener(AUTH_UNAUTHORIZED_EVENT, clearSession);
  }, [clearSession]);

  const login = useCallback(async (credentials: LoginRequest) => {
    const authenticatedUser = await authService.login(credentials);
    setToken(authService.getToken());
    setUser(authenticatedUser);
    setIsLoading(false);
    return authenticatedUser;
  }, []);

  const logout = clearSession;

  const hasPermission = useCallback(
    (permission: string) => user?.permissions.includes(permission) ?? false,
    [user],
  );

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      token,
      isAuthenticated: Boolean(token && user),
      isLoading,
      login,
      logout,
      refreshCurrentUser,
      hasPermission,
    }),
    [hasPermission, isLoading, login, logout, refreshCurrentUser, token, user],
  );

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}
