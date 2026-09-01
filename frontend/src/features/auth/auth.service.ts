import { httpClient } from "../../api/httpClient";
import type {
  AuthUser,
  ChangePasswordRequest,
  LoginRequest,
  LoginResponse,
} from "./auth.types";

const TOKEN_KEY = "rodalltv_token";
const LEGACY_USER_KEY = "rodalltv_user";

function mapAuthUser(data: AuthUser | LoginResponse): AuthUser {
  return {
    userId: data.userId || ("user" in data ? data.user ?? "" : ""),
    firstName: data.firstName ?? "",
    lastName: data.lastName ?? "",
    email: data.email,
    role: data.role,
    mustChangePassword: data.mustChangePassword ?? false,
    permissions: data.permissions ?? [],
  };
}

export const authService = {
  async login(credentials: LoginRequest): Promise<AuthUser> {
    const response = await httpClient.post<LoginResponse>(
      "/api/Auth/login",
      credentials,
    );
    localStorage.setItem(TOKEN_KEY, response.data.token);
    localStorage.removeItem(LEGACY_USER_KEY);

    return mapAuthUser(response.data);
  },

  async getCurrentUser(): Promise<AuthUser> {
    const response = await httpClient.get<AuthUser>("/api/Auth/me");
    return mapAuthUser(response.data);
  },

  async changePassword(payload: ChangePasswordRequest): Promise<void> {
    await httpClient.post("/api/Auth/change-password", payload);
  },

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(LEGACY_USER_KEY);
  },

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  },

};
