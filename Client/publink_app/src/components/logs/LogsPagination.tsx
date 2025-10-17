import React from 'react';

type Props = {
  pageNumber: number;
  totalPages?: number;
  totalCount?: number;
  canGoPrev: boolean;
  canGoNext: boolean;
  onChangePage: (nextPage: number) => void;
};

const LogsPagination: React.FC<Props> = ({ pageNumber, totalPages, totalCount, canGoPrev, canGoNext, onChangePage }) => {
  return (
    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: 12 }}>
      <button onClick={() => onChangePage(pageNumber - 1)} disabled={!canGoPrev}>
        Previous
      </button>
      <span>
        Page {pageNumber}
        {totalPages ? ` of ${totalPages}` : ''}
        {typeof totalCount === 'number' ? ` • Total: ${totalCount}` : ''}
      </span>
      <button onClick={() => onChangePage(pageNumber + 1)} disabled={!canGoNext}>
        Next
      </button>
    </div>
  );
};

export default LogsPagination;
