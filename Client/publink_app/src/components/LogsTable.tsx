import { useEffect } from 'react';
import LogsToolbar from './logs/LogsToolbar';
import LogsTableHeader from './logs/LogsTableHeader';
import LogsPagination from './logs/LogsPagination';
import { renderEntityBadge, renderTypeBadge } from './logs/Badges';
import { formatMmSs } from '../utils/time';
import { useDispatch, useSelector } from 'react-redux';
import type {AppDispatch} from '../store/store';
import {
  fetchLogs,
  fetchOrganisations,
  selectCurrentQuery,
  selectError,
  selectLoading,
  selectPageData,
  selectOrganizations,
  selectSelectedOrganizationId,
  setPage,
  setPageSize,
  setSort,
  setSelectedOrganization,
} from '../features/logs/logsSlice';

const columns = [
  { key: 'CreatedDate', header: 'Created', sortable: true },
  { key: 'Type', header: 'Type', sortable: true },
  { key: 'EntityType', header: 'Entity Type', sortable: true },
  { key: 'ContractNumber', header: 'Contract Number', sortable: false },
  { key: 'ChangedBy', header: 'Changed By', sortable: false },
  { key: 'ProcessTookTime', header: 'Duration (mm:ss)', sortable: false },
  { key: 'EntitiesAffectCount', header: 'Affected Entities', sortable: false },
];


// Base component that renders a simple table; manages sorting/pagination via Redux
function LogsTableBase() {
  const dispatch = useDispatch<AppDispatch>();
  const query = useSelector(selectCurrentQuery);
  const pageData = useSelector(selectPageData);
  const loading = useSelector(selectLoading);
  const error = useSelector(selectError);
  const organizations = useSelector(selectOrganizations);
  const selectedOrganizationId = useSelector(selectSelectedOrganizationId);

  // fetch organizations on mount
  useEffect(() => {
    dispatch(fetchOrganisations());
  }, [dispatch]);

  // fetch logs when organization or query params change
  useEffect(() => {
    if (selectedOrganizationId) {
      dispatch(fetchLogs({
        organizationId: selectedOrganizationId,
        pageNumber: query.pageNumber,
        pageSize: query.pageSize,
        sortColumn: query.sortColumn,
        sortDirection: query.sortDirection,
      }));
    }
  }, [dispatch, selectedOrganizationId, query.pageNumber, query.pageSize, query.sortColumn, query.sortDirection]);

  const items = pageData?.items ?? [];

  const totalCount = typeof pageData?.totalCount === 'number' ? pageData.totalCount : undefined;
  const totalPages = totalCount !== undefined ? Math.max(1, Math.ceil(totalCount / query.pageSize)) : undefined;
  const canGoPrev = query.pageNumber > 1;
  const canGoNext = totalPages === undefined ? true : query.pageNumber < totalPages;

  const formatDate = (iso?: string) => {
    if (!iso) return '';
    const d = new Date(iso);
    if (isNaN(d.getTime())) return iso as any;
    return d.toLocaleString();
  };


  const onHeaderClick = (key: string) => {
    const isCurrent = query.sortColumn === key;
    const direction = isCurrent && query.sortDirection === 'desc' ? 'asc' : 'desc';
    dispatch(setSort({ column: key, direction }));
    dispatch(fetchLogs({ sortColumn: key, sortDirection: direction, pageNumber: 1, organizationId: selectedOrganizationId ?? undefined }));
  };

  const onChangePage = (nextPage: number) => {
    if (nextPage < 1) return;
    if (totalPages && nextPage > totalPages) return;
    dispatch(setPage(nextPage));
    dispatch(fetchLogs({ pageNumber: nextPage, organizationId: selectedOrganizationId ?? undefined }));
  };

  const onChangePageSize = (size: number) => {
    dispatch(setPageSize(size));
    dispatch(fetchLogs({ pageSize: size, pageNumber: 1, organizationId: selectedOrganizationId ?? undefined }));
  };
  
  return (
    <div>
      <LogsToolbar
        title="Audit Logs"
        organizations={organizations as any}
        selectedOrganizationId={selectedOrganizationId ?? null}
        pageSize={query.pageSize}
        onChangeOrganization={(id) => {
          dispatch(setSelectedOrganization(id));
          if (id) {
            dispatch(fetchLogs({ organizationId: id, pageNumber: 1 }));
          }
        }}
        onChangePageSize={onChangePageSize}
      />

      {error && (
        <div style={{ color: 'red', padding: '8px 0' }}>Error: {error}</div>
      )}

      <div style={{ position: 'relative' }} aria-busy={loading} aria-live="polite">
        <table className="ui celled table" style={{ width: '100%', margin: 0 }}>
          <LogsTableHeader
            columns={columns}
            sortColumn={query.sortColumn}
            sortDirection={query.sortDirection as any}
            onHeaderClick={onHeaderClick}
          />
          <tbody style={{ backgroundColor: '#272727' }}>
            {items.length === 0 && !loading ? (
              <tr>
                <td colSpan={columns.length} style={{ textAlign: 'center', color: 'red' }}>
                  No data
                </td>
              </tr>
            ) : (
              items.map((row: any, idx: number) => (
                <tr key={row.Id ?? idx}>
                  <td>{formatDate(row.CreatedDate)}</td>
                  <td>{renderTypeBadge(row.Type)}</td>
                  <td>{renderEntityBadge(row.EntityType)}</td>
                  <td>{row.ContractNumber ?? ''}</td>
                  <td>{row.ChangedBy ?? row.UserEmail ?? ''}</td>
                  <td>{formatMmSs(row.ProcessTookTime)}</td>
                  <td>{row.EntitiesAffectCount ?? ''}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>

        {loading && (
          <div
            role="status"
            aria-label="Loading"
            style={{
              position: 'absolute',
              inset: 0,
              background: 'rgba(0,0,0,0.35)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              backdropFilter: 'blur(1px)',
              WebkitBackdropFilter: 'blur(1px)',
              transition: 'opacity 150ms ease-out',
              boxShadow: 'inset 0 0 0 1px rgba(255,255,255,0.03)'
            }}
          >
            <div style={{ padding: 16, borderRadius: 12, background: 'rgba(17, 24, 39, 0.6)', boxShadow: '0 10px 25px rgba(0,0,0,0.35)' }}>
              <svg width="56" height="56" viewBox="0 0 50 50" xmlns="http://www.w3.org/2000/svg" style={{ display: 'block' }}>
                <circle cx="25" cy="25" r="20" fill="none" stroke="rgba(255,255,255,0.15)" strokeWidth="5" />
                <path d="M25 5 a20 20 0 0 1 0 40" fill="none" stroke="#3b82f6" strokeWidth="5" strokeLinecap="round">
                  <animateTransform attributeName="transform" type="rotate" from="0 25 25" to="360 25 25" dur="0.8s" repeatCount="indefinite" />
                </path>
              </svg>
            </div>
          </div>
        )}
      </div>

      <LogsPagination
        pageNumber={query.pageNumber}
        totalPages={totalPages}
        totalCount={totalCount}
        canGoPrev={canGoPrev}
        canGoNext={canGoNext}
        onChangePage={onChangePage}
      />
    </div>
  );
}

// Export the base component directly; sematable has been removed
export default LogsTableBase;
