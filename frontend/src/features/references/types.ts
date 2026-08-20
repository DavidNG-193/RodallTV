export type ReferenceLookup = {
  referenceNumber: string;
  referenceDate: string;
  client: string;
  operationCode: string;
  operation: string;
  document: string;
  customsOfficeNumber: number;
  customsOffice: string;
  statusCode: string;
  status: string;
};

export type DailyReference = ReferenceLookup & {
  id: string;
  lastExternalUpdateAt: string;
  createdAt: string;
};

export type RefreshReferencesResponse = {
  totalCount: number;
  refreshedCount: number;
  notFoundCount: number;
  failedCount: number;
  refreshedAtUtc: string;
};
