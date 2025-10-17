import React from 'react';

export type OrganizationOption = { id: string; name: string };

type Props = {
  title?: string;
  organizations: OrganizationOption[];
  selectedOrganizationId: string | null;
  pageSize: number;
  onChangeOrganization: (id: string | null) => void;
  onChangePageSize: (size: number) => void;
};

const LogsToolbar: React.FC<Props> = ({
  title = 'Audit Logs',
  organizations,
  selectedOrganizationId,
  pageSize,
  onChangeOrganization,
  onChangePageSize,
}) => {
    // Add more page sizes as needed
    const pageSizeList = [10];

  return (
    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
      <h2>{title}</h2>
      <div>
        <label style={{ marginRight: 12 }}>
          Organization:
          <select
            style={{ marginLeft: 8, maxWidth: '300px' }}
            value={selectedOrganizationId ?? ''}
            onChange={(e) => onChangeOrganization(e.target.value || null)}
            disabled={!organizations.length}
          >
            {organizations.map((o) => (
              <option key={o.id} value={o.id}>
                {o.name}
              </option>
            ))}
          </select>
        </label>
        <label>
          Page size:
          <select value={pageSize} onChange={(e) => onChangePageSize(parseInt(e.target.value, 10))} style={{ marginLeft: 8 }}>
            {pageSizeList.map((s) => (
              <option key={s} value={s}>
                {s}
              </option>
            ))}
          </select>
        </label>
      </div>
    </div>
  );
};

export default LogsToolbar;
