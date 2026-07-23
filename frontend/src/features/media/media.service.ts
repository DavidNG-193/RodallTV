import { httpClient } from "../../api/httpClient";
import type {
  MediaItem,
  UploadMediaRequest,
} from "./media.types";

async function fetchMediaFile(id: string): Promise<Blob> {
  const response = await httpClient.get<Blob>(
    `/api/Media/${id}/file`,
    {
      responseType: "blob",
    },
  );

  return response.data;
}

export const mediaService = {
  async getAll(): Promise<MediaItem[]> {
    const response =
      await httpClient.get<MediaItem[]>(
        "/api/Media",
      );

    return response.data;
  },

  async upload(
    request: UploadMediaRequest,
  ): Promise<MediaItem> {
    const formData = new FormData();

    formData.append(
      "file",
      request.file,
    );

    if (request.mediaFolderId) {
      formData.append(
        "mediaFolderId",
        request.mediaFolderId,
      );
    }

    const response =
      await httpClient.post<MediaItem>(
        "/api/Media",
        formData,
        {
          headers: {
            "Content-Type":
              "multipart/form-data",
          },
        },
      );

    return response.data;
  },

  async deactivate(
    id: string,
  ): Promise<void> {
    await httpClient.delete(
      `/api/Media/${id}`,
    );
  },

  async getFile(id: string): Promise<Blob> {
    return fetchMediaFile(id);
  },

  async download(
    media: MediaItem,
  ): Promise<void> {
    const file = await fetchMediaFile(media.id);
    const objectUrl = URL.createObjectURL(file);
    const anchor = document.createElement("a");

    anchor.href = objectUrl;
    anchor.download = media.originalFileName;

    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();

    URL.revokeObjectURL(objectUrl);
  },
};
