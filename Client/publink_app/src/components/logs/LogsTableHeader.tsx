import React, { useMemo } from 'react';

export type Column = { key: string; header: string; sortable?: boolean };

type Props = {
  columns: Column[];
  sortColumn?: string;
  sortDirection?: 'asc' | 'desc';
  onHeaderClick: (key: string) => void;
};

const LogsTableHeader: React.FC<Props> = ({ columns, sortColumn, sortDirection, onHeaderClick }) => {
  const headerCells = useMemo(() => {
    return columns.map((col) => (
      <th
        key={col.key}
        style={{ cursor: col.sortable ? 'pointer' : 'default', color: sortColumn === col.key ? 'white' : 'lightgrey', padding: '8px 12px', backgroundColor: '#1a1a1a' }}
        onClick={() => col.sortable && onHeaderClick(col.key)}
      >
        {col.header}
        {sortColumn === col.key ? (sortDirection === 'asc' ? '\u2191' : '\u2193') : ''}
      </th>
    ));
  }, [columns, sortColumn, sortDirection, onHeaderClick]);

  return (
    <thead style={{ backgroundColor: '#f3f4f6' }}>
      <tr>{headerCells}</tr>
    </thead>
  );
};

export default LogsTableHeader;
