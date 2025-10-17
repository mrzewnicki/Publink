export type SortDirection = 'asc' | 'desc';

export interface LogsQuery {
  pageNumber: number;
  pageSize: number;
  sortColumn: string; // e.g., 'CreatedDate'
  sortDirection: SortDirection; // 'asc' | 'desc'
  organizationId?: string | null; // Guid of selected organization
}

// Mirrors the AuditLogDto returned by the API
export interface AuditLogDto {
  Id: number;
  OrganizationId?: string | null; // Guid
  UserId?: string | null; // Guid
  UserEmail?: string | null;
  Type: number;
  EntityType: number;
  CreatedDate: string; // ISO date string
  OldValues?: string | null;
  NewValues?: string | null;
  AffectedColumns?: string | null;
  PrimaryKey?: string | null;
  EntityId?: string | null; // Guid
  ParentId?: string | null; // Guid
  CorrelationId?: string | null; // Guid
  SubUnitId?: string | null; // Guid
}

export interface LogsPage {
  items: AuditLogDto[];
  totalCount?: number;
}

export interface Organization {
  id: string;
  name: string;
}

export interface LogsState {
  current: LogsQuery;
  cache: {
    // key: `${organizationId}|${sortColumn}:${sortDirection}` -> page -> LogsPage
    [sortKey: string]: {
      [pageNumber: number]: LogsPage;
    };
  };
  // Points to the last successfully displayed page; used to keep rows visible during loading
  lastShownSortKey?: string | null;
  lastShownPage?: number | null;
  organizations: Organization[];
  selectedOrganizationId?: string | null;
  loading: boolean;
  error?: string | null;
}
