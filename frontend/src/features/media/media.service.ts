import { httpClient } from "../../api/httpClient";
import type {
  MediaItem,
  MediaQuery,
  PagedMediaResponse,
  UploadMediaRequest,
  UpdateMediaRequest,
} from "./media.types";

async function fetchMediaFile(id: string): Promise<Blob> {
  const response = await httpClient.get<Blob>(
    `/api/Media/${id}/file`,
    {
      responseType: "blob",
      timeout: 0,
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

  async getPaged(query: MediaQuery): Promise<PagedMediaResponse> {
    const response = await httpClient.get<PagedMediaResponse>(
      "/api/Media/paged",
      {
        params: {
          page: query.page,
          pageSize: query.pageSize,
          search: query.search || undefined,
          mediaType: query.mediaType === "all" ? undefined : query.mediaType,
          mediaFolderId: query.mediaFolderId || undefined,
          rootOnly: query.rootOnly || undefined,
          sort: query.sort,
        },
      },
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

  async update(
    id: string,
    request: UpdateMediaRequest,
  ): Promise<MediaItem> {
    const response = await httpClient.put<MediaItem>(
      `/api/Media/${id}`,
      request,
    );

    return response.data;
  },

  async getFile(id: string): Promise<Blob> {
    return fetchMediaFile(id);
  },

  async getThumbnail(id: string): Promise<Blob> {
    const response = await httpClient.get<Blob>(
      `/api/Media/${id}/thumbnail`,
      {
        responseType: "blob",
        timeout: 0,
      },
    );

    return response.data;
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
