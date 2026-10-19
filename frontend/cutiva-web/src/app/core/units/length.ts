/** Single config point: UI shows centimeters; API stores millimeters. */
export const UI_LENGTH_UNIT = 'cm' as const;
export const MM_PER_CM = 10;

/** Default market availability so the optimizer can plan purchases without warehouse qty. */
export const MARKET_UNLIMITED_QTY = 9999;

export function mmToCm(mm: number): number {
  return mm / MM_PER_CM;
}

export function cmToMm(cm: number): number {
  return Math.round(cm * MM_PER_CM);
}

export function formatCm(mm: number, digits = 1): string {
  const cm = mmToCm(mm);
  return Number.isInteger(cm) ? String(cm) : cm.toFixed(digits);
}
