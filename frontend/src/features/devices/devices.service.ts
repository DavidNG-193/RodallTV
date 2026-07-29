import { httpClient } from "../../api/httpClient";
import type {
  CreateDeviceRequest,
  CreateDeviceResponse,
  Device,
  DeviceListFilter,
  PowerCommandType,
  SendPowerCommandResponse,
  UpdateDeviceRequest,
} from "./devices.types";

export const devicesService = {
  async getAll(
    filter: DeviceListFilter = "active",
  ): Promise<Device[]> {
    const response = await httpClient.get<Device[]>("/api/Devices", {
      params: {
        status: filter,
      },
    });

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

  async reactivate(id: string): Promise<Device> {
    const response = await httpClient.patch<Device>(
      `/api/Devices/${id}/reactivate`,
    );

    return response.data;
  },

  async sendPowerCommand(
    deviceId: string,
    commandType: PowerCommandType,
  ): Promise<SendPowerCommandResponse> {
    const response =
      await httpClient.post<SendPowerCommandResponse>(
        `/api/Devices/${deviceId}/power-command`,
        { commandType },
      );

    return response.data;
  },
};
