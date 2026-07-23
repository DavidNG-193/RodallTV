export interface MediaFolder {
  id: string;
  name: string;
  description: string | null;
  createdAt: string;
  createdByUserId: string;
  isActive: boolean;
}

export interface CreateMediaFolderRequest {
  name: string;
  description?: string | null;
}

export interface UpdateMediaFolderRequest {
  name: string;
  description?: string | null;
}