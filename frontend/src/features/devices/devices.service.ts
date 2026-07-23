import { httpClient } from "../../api/httpClient";
import type {
  CreateDeviceRequest,
  CreateDeviceResponse,
  Device,
  UpdateDeviceRequest,
} from "./devices.types";

export const devicesService = {
  async getAll(): Promise<Device[]> {
    const response = await httpClient.get<Device[]>("/api/Devices");
    return response.data;
  },

  async create(
    request: CreateDeviceRequest,
  ): Promise<CreateDeviceResponse> {
    const response = await httpClient.post<CreateDeviceResponse>(
      "/api/Devices",
      request,
    );

    return response.data;
  },

  async update(
    id: string,
    request: UpdateDeviceRequest,
  ): Promise<Device> {
    const response = await httpClient.put<Device>(
      `/api/Devices/${id}`,
      request,
    );

    return response.data;
  },

  async deactivate(id: string): Promise<void> {
    await httpClient.delete(`/api/Devices/${id}`);
  },
};