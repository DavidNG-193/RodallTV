import { httpClient } from "../../api/httpClient";
import type {
  CreateUserPayload,
  UpdateUserPayload,
  UserDetail,
  UserListItem,
} from "./users.types";

export const usersService = {
  async getUsers(): Promise<UserListItem[]> {
    const response = await httpClient.get<UserListItem[]>("/api/users");
    return response.data;
  },

  async getUserById(id: string): Promise<UserDetail> {
    const response = await httpClient.get<UserDetail>(`/api/users/${id}`);
    return response.data;
  },

  async createUser(payload: CreateUserPayload): Promise<UserDetail> {
    const response = await httpClient.post<UserDetail>("/api/users", payload);
    return response.data;
  },

  async updateUser(id: string, payload: UpdateUserPayload): Promise<UserDetail> {
    const response = await httpClient.put<UserDetail>(`/api/users/${id}`, payload);
    return response.data;
  },

  async updateUserStatus(id: string, isActive: boolean): Promise<void> {
    await httpClient.patch(`/api/users/${id}/status`, { isActive });
  },

  async resetUserPassword(id: string, password: string): Promise<void> {
    await httpClient.post(`/api/users/${id}/reset-password`, {
      password,
    });
  },
};
