import React from 'react';

// Small badge component with configurable colors
export const Badge: React.FC<{ bg: string; fg?: string; title?: string; children: React.ReactNode }> = ({ bg, fg, title, children }) => (
  <span
    title={title}
    style={{
      display: 'inline-block',
      padding: '2px 8px',
      borderRadius: 999,
      fontSize: 12,
      fontWeight: 600,
      lineHeight: 1.5,
      backgroundColor: bg,
      color: fg ?? '#fff',
      whiteSpace: 'nowrap',
    }}
  >
    {children}
  </span>
);

// helpers to map numeric/strings to labels and colors
const TYPE_MAP: Record<string, { label: string; bg: string; fg?: string }> = {
  '1': { label: 'Added', bg: '#22c55e' }, // green
  '2': { label: 'Deleted', bg: '#ef4444' }, // red
  '3': { label: 'Modified', bg: '#f59e0b', fg: '#111827' } // yellow with dark text
};

const ENTITY_MAP: Record<string, { label: string; bg: string; fg?: string }> = {
  '0': { label: 'Unknown', bg: '#9ca3af' }, // gray
  '1': { label: 'ContractHeaderEntity', bg: '#3b82f6' }, // blue
  '2': { label: 'AnnexHeaderEntity', bg: '#3b82f6' }, // blue
  '3': { label: 'AnnexChangeEntity', bg: '#3b82f6' }, // blue
  '4': { label: 'FileEntity', bg: '#3b82f6' }, // blue
  '5': { label: 'InvoiceEntity', bg: '#3b82f6' }, // blue
  '6': { label: 'PaymentScheduleEntity', bg: '#3b82f6' }, // blue
  '7': { label: 'ContractFundingEntity', bg: '#3b82f6' }, // blue
  unknown: { label: 'Unknown', bg: '#9ca3af' }
};

export function renderTypeBadge(val: any) {
  if (val === null || val === undefined) return '' as any;
  const key = (typeof val === 'number' ? String(val) : String(val).trim()).toLowerCase();
  const meta = TYPE_MAP[key] ?? { label: String(val), bg: '#9ca3af' };
  return <Badge bg={meta.bg} fg={meta.fg} title={String(val)}>{meta.label}</Badge>;
}

export function renderEntityBadge(val: any) {
  if (val === null || val === undefined) return '' as any;
  const key = (typeof val === 'number' ? String(val) : String(val).trim()).toLowerCase();
  const meta = ENTITY_MAP[key] ?? { label: String(val), bg: '#3b82f6' };
  return <Badge bg={meta.bg} fg={meta.fg} title={String(val)}>{meta.label}</Badge>;
}
