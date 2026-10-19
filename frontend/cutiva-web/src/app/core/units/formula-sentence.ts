import { cmToMm, mmToCm } from './length';

export type FormulaAxis = 'width' | 'height';
export type FormulaOp = 'plus' | 'minus';

/** Workshop sentence controls mapped to widthCoef / heightCoef / offsetMm. */
export interface SimpleFormula {
  axis: FormulaAxis;
  op: FormulaOp;
  nCm: number;
}

export function isSimpleFormula(
  widthCoef: number,
  heightCoef: number,
): boolean {
  const w = Number(widthCoef);
  const h = Number(heightCoef);
  return (w === 1 && h === 0) || (w === 0 && h === 1);
}

export function toSimpleFormula(
  widthCoef: number,
  heightCoef: number,
  offsetMm: number,
): SimpleFormula | null {
  if (!isSimpleFormula(widthCoef, heightCoef)) return null;
  const axis: FormulaAxis = Number(widthCoef) === 1 ? 'width' : 'height';
  const offset = Number(offsetMm) || 0;
  return {
    axis,
    op: offset >= 0 ? 'plus' : 'minus',
    nCm: Math.abs(mmToCm(offset)),
  };
}

export function applySimpleFormula(simple: SimpleFormula): {
  widthCoef: number;
  heightCoef: number;
  offsetMm: number;
} {
  const nMm = cmToMm(Math.abs(Number(simple.nCm) || 0));
  const offsetMm = simple.op === 'minus' ? -nMm : nMm;
  if (simple.axis === 'width') {
    return { widthCoef: 1, heightCoef: 0, offsetMm };
  }
  return { widthCoef: 0, heightCoef: 1, offsetMm };
}

export function computeLengthMm(opts: {
  widthMm: number;
  heightMm: number;
  widthCoef: number;
  heightCoef: number;
  offsetMm: number;
  useSashWidthDivision?: boolean;
  sashCount?: number | null;
  overlapMm?: number | null;
}): number {
  let w = opts.widthMm;
  if (
    opts.useSashWidthDivision &&
    opts.sashCount != null &&
    opts.sashCount > 0
  ) {
    w = opts.widthMm / opts.sashCount + (opts.overlapMm ?? 0);
  }
  return (
    Number(opts.widthCoef) * w +
    Number(opts.heightCoef) * opts.heightMm +
    Number(opts.offsetMm)
  );
}
