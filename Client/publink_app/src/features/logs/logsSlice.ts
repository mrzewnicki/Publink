import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import type { LogsPage, LogsQuery, LogsState, SortDirection, Organization } from './types';
import type { RootState } from '../../store/store';

const API_BASE = 'http://localhost:5161/api/Logs';
const ORGS_API = 'http://localhost:5161/api/Organisations';

function makeSortKey(orgId: string | null | undefined, sortColumn: string, sortDirection: SortDirection) {
  return `${orgId ?? 'none'}|${sortColumn}:${sortDirection}`;
}

// Normalize API items to ensure PascalCase keys expected by UI are present.
// Many .NET APIs serialize to camelCase (createdDate, userEmail, etc.).
// This function adds PascalCase aliases if missing so the table renders values.
function normalizeAuditLog(o: any) {
  const n: any = { ...o };
  const pairs: Array<[string, string]> = [
    ['id', 'Id'],
    ['organizationId', 'OrganizationId'],
    ['userId', 'UserId'],
    ['userEmail', 'UserEmail'],
    ['changedBy', 'ChangedBy'],
    ['type', 'Type'],
    ['entityType', 'EntityType'],
    ['contractNumber', 'ContractNumber'],
    ['createdDate', 'CreatedDate'],
    ['processTookTime', 'ProcessTookTime'],
    ['entitiesAffectCount', 'EntitiesAffectCount'],
    ['oldValues', 'OldValues'],
    ['newValues', 'NewValues'],
    ['affectedColumns', 'AffectedColumns'],
    ['primaryKey', 'PrimaryKey'],
    ['entityId', 'EntityId'],
    ['parentId', 'ParentId'],
    ['correlationId', 'CorrelationId'],
    ['subUnitId', 'SubUnitId'],
  ];
  for (const [src, dst] of pairs) {
    if (n[dst] === undefined && n[src] !== undefined) {
      n[dst] = n[src];
    }
  }
  return n;
}

export const fetchOrganisations = createAsyncThunk<
  Organization[],
  void,
  { state: RootState }
>('logs/fetchOrganisations', async () => {
  const res = await fetch(ORGS_API);
  if (!res.ok) {
    throw new Error(`Failed to fetch organizations (${res.status}): ${await res.text()}`);
  }
  const json = await res.json();
  const arr = Array.isArray(json?.items) ? json.items : Array.isArray(json) ? json : [];
  const normalized: Organization[] = arr.map((o: any) => {
    const id = o?.id ?? o?.Id ?? o?.organizationId ?? o?.OrganizationId;
    const name = o?.name ?? o?.Name ?? o?.organizationName ?? o?.OrganizationName;
    return { id: String(id), name: String(name ?? id ?? '') };
  }).filter((o: Organization) => !!o.id);
  return normalized;
});

export const fetchLogs = createAsyncThunk<
  { page: number; sortKey: string; data: LogsPage },
  Partial<LogsQuery> | undefined,
  { state: RootState }
>('logs/fetchLogs', async (partial, thunkApi) => {
  const state = thunkApi.getState();
  const { current, selectedOrganizationId } = state.logs;
  const organizationId = partial?.organizationId ?? current.organizationId ?? selectedOrganizationId ?? null;
  const query: LogsQuery = {
    pageNumber: partial?.pageNumber ?? current.pageNumber,
    pageSize: partial?.pageSize ?? current.pageSize,
    sortColumn: partial?.sortColumn ?? current.sortColumn,
    sortDirection: partial?.sortDirection ?? current.sortDirection,
    organizationId,
  };

  const sortKey = makeSortKey(query.organizationId, query.sortColumn, query.sortDirection);

  // If we already have this page cached under the same org+sort, avoid refetch
  const cached = state.logs.cache[sortKey]?.[query.pageNumber];
  if (cached) {
    return { page: query.pageNumber, sortKey, data: cached };
  }

  // If no organization selected yet, return empty data (component will re-fetch after selection)
  if (!query.organizationId) {
    return { page: query.pageNumber, sortKey, data: { items: [], totalCount: 0 } };
  }

  const params = new URLSearchParams();
  params.set('pageNumber', String(query.pageNumber));
  params.set('pageSize', String(query.pageSize));
  params.set('sort', `${query.sortColumn}:${query.sortDirection}`);
  params.set('organizationId', String(query.organizationId));

  const url = `${API_BASE}?${params.toString()}`;
  const res = await fetch(url);
  if (!res.ok) {
    throw new Error(`Failed to fetch logs (${res.status}): ${await res.text()}`);
  }
  const json = await res.json();

  // Try to map the most common shapes
  const rawItems = Array.isArray(json?.items) ? json.items : Array.isArray(json) ? json : [];
  const items = rawItems.map(normalizeAuditLog);
  const totalCount = typeof json?.totalCount === 'number' ? json.totalCount : undefined;
  const data: LogsPage = { items, totalCount };

  return { page: query.pageNumber, sortKey, data };
});

const initialState: LogsState = {
  current: {
    pageNumber: 1,
    pageSize: 20,
    sortColumn: 'CreatedDate',
    sortDirection: 'desc',
    organizationId: null,
  },
  cache: {},
  organizations: [],
  selectedOrganizationId: null,
  loading: false,
  error: null,
};

const logsSlice = createSlice({
  name: 'logs',
  initialState,
  reducers: {
    setSort(state, action: PayloadAction<{ column: string; direction: SortDirection }>) {
      const { column, direction } = action.payload;
      state.current.sortColumn = column;
      state.current.sortDirection = direction;
      state.current.pageNumber = 1; // reset page when sort changes
    },
    setPage(state, action: PayloadAction<number>) {
      state.current.pageNumber = action.payload;
    },
    setPageSize(state, action: PayloadAction<number>) {
      state.current.pageSize = action.payload;
      state.current.pageNumber = 1;
    },
    setSelectedOrganization(state, action: PayloadAction<string | null>) {
      state.selectedOrganizationId = action.payload ?? null;
      state.current.organizationId = state.selectedOrganizationId;
      state.current.pageNumber = 1;
      state.cache = {};
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchOrganisations.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchOrganisations.fulfilled, (state, action) => {
        state.loading = false;
        state.organizations = action.payload;
        if (!state.selectedOrganizationId && state.organizations.length > 0) {
          const last = state.organizations[state.organizations.length - 1];
          state.selectedOrganizationId = last.id;
          state.current.organizationId = state.selectedOrganizationId;
          state.current.pageNumber = 1;
          state.cache = {};
        }
      })
      .addCase(fetchOrganisations.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to load organizations';
      })
      .addCase(fetchLogs.pending, (state, action) => {
        state.loading = true;
        state.error = null;
        // apply incoming partial to current if provided
        const partial = action.meta.arg;
        if (partial) {
          if (partial.pageNumber !== undefined) state.current.pageNumber = partial.pageNumber;
          if (partial.pageSize !== undefined) state.current.pageSize = partial.pageSize;
          if (partial.sortColumn !== undefined) state.current.sortColumn = partial.sortColumn;
          if (partial.sortDirection !== undefined) state.current.sortDirection = partial.sortDirection;
          if (partial.organizationId !== undefined) state.current.organizationId = partial.organizationId;
        }
      })
      .addCase(fetchLogs.fulfilled, (state, action) => {
        state.loading = false;
        const { page, sortKey, data } = action.payload;
        if (!state.cache[sortKey]) state.cache[sortKey] = {};
        state.cache[sortKey][page] = data;
      })
      .addCase(fetchLogs.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Unknown error';
      });
  },
});

export const { setSort, setPage, setPageSize, setSelectedOrganization } = logsSlice.actions;

export const selectCurrentQuery = (state: RootState) => state.logs.current;
export const selectOrganizations = (state: RootState) => state.logs.organizations;
export const selectSelectedOrganizationId = (state: RootState) => state.logs.selectedOrganizationId;
export const selectCurrentSortKey = (state: RootState) => {
  const s = state.logs.current;
  return `${s.organizationId ?? state.logs.selectedOrganizationId ?? 'none'}|${s.sortColumn}:${s.sortDirection}`;
};
export const selectPageData = (state: RootState) => {
  const s = state.logs.current;
  const sortKey = `${s.organizationId ?? state.logs.selectedOrganizationId ?? 'none'}|${s.sortColumn}:${s.sortDirection}`;
  return state.logs.cache[sortKey]?.[state.logs.current.pageNumber];
};
export const selectLoading = (state: RootState) => state.logs.loading;
export const selectError = (state: RootState) => state.logs.error;

export default logsSlice.reducer;
