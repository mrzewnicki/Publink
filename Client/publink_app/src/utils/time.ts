// Utility functions for time/duration formatting

// Format various duration-like inputs to mm:ss
// - number: treated as milliseconds
// - string: supports "HH:mm:ss(.fff)", "mm:ss", ISO 8601 duration (e.g., PT1M23S),
//   and Date-parsable strings (uses UTC minutes/seconds)
// - Date-like values: attempted via new Date(value)
export function formatMmSs(value: any): string {
  if (value === null || value === undefined) return '';
  // If it's a number, assume milliseconds
  if (typeof value === 'number' && isFinite(value)) {
    const totalSeconds = Math.max(0, Math.floor(value / 1000));
    const mm = Math.floor(totalSeconds / 60);
    const ss = totalSeconds % 60;
    return `${String(mm).padStart(2, '0')}:${String(ss).padStart(2, '0')}`;
  }
  if (typeof value === 'string') {
    const s = value.trim();
    // Try HH:mm:ss(.fff) or mm:ss
    const timeMatch = s.match(/^(\d{1,2}):(\d{2})(?::(\d{2})(?:\.\d{1,7})?)?$/);
    if (timeMatch) {
      const hh = parseInt(timeMatch[1] || '0', 10);
      const mm = parseInt(timeMatch[2] || '0', 10);
      const ss = parseInt(timeMatch[3] || '0', 10);
      const totalMinutes = hh * 60 + mm;
      return `${String(totalMinutes).padStart(2, '0')}:${String(ss).padStart(2, '0')}`;
    }
    // Try ISO 8601 duration like PT1M23S
    const isoDur = s.match(/^P(T(?:(\d+)H)?(?:(\d+)M)?(?:(\d+)S)?)$/i);
    if (isoDur) {
      const hh = parseInt(isoDur[2] || '0', 10);
      const mm = parseInt(isoDur[3] || '0', 10);
      const ss = parseInt(isoDur[4] || '0', 10);
      const totalMinutes = hh * 60 + mm;
      return `${String(totalMinutes).padStart(2, '0')}:${String(ss).padStart(2, '0')}`;
    }
    // Try parseable date string
    const d = new Date(s);
    if (!isNaN(d.getTime())) {
      const mm = d.getUTCMinutes();
      const ss = d.getUTCSeconds();
      return `${String(mm).padStart(2, '0')}:${String(ss).padStart(2, '0')}`;
    }
    return s;
  }
  // Fallback: attempt Date
  const d = new Date(value);
  if (!isNaN(d.getTime())) {
    const mm = d.getUTCMinutes();
    const ss = d.getUTCSeconds();
    return `${String(mm).padStart(2, '0')}:${String(ss).padStart(2, '0')}`;
  }
  return String(value);
}
