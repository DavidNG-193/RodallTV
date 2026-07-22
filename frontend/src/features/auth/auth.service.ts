import { httpClient } from "../../api/httpClient";
import type { AuthUser, LoginRequest, LoginResponse } from "./auth.types";

const TOKEN_KEY = "rodalltv_token";
const USER_KEY = "rodalltv_user";

export const authService = {
  async login(credentials: LoginRequest): Promise<AuthUser> {
    const response = await httpClient.post<LoginResponse>(
      "/api/Auth/login",
      credentials,
    );
    const user: AuthUser = {
      id: response.data.user,
      email: response.data.email,
      role: response.data.role,
    };

    localStorage.setItem(TOKEN_KEY, response.data.token);
    localStorage.setItem(USER_KEY, JSON.stringify(user));

    return user;
  },

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  },

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  },

  getStoredUser(): AuthUser | null {
    const storedUser = localStorage.getItem(USER_KEY);

    if (!storedUser) {
      return null;
    }

    try {
      return JSON.parse(storedUser) as AuthUser;
    } catch {
      localStorage.removeItem(USER_KEY);
      return null;
    }
  },

  isAuthenticated(): boolean {
    return Boolean(localStorage.getItem(TOKEN_KEY));
  },
};
