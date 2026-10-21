import type { ProductKind } from '../../core/api/product-templates.api';
import type { PreviewLayout, PreviewPaneStyle } from './frame-preview';

export interface PreviewRecipe {
  kind?: ProductKind | null;
  code?: string | null;
  name?: string | null;
  sashCount?: number | null;
  hasGlass: boolean;
}

export interface ResolvedPreview {
  layout: PreviewLayout;
  paneStyle: PreviewPaneStyle;
  sectorCode: string;
}

/** One mapping from a product recipe to the frame drawing. */
export function resolveFramePreview(recipe: PreviewRecipe): ResolvedPreview {
  const code = (recipe.code ?? '').toUpperCase();
  const name = (recipe.name ?? '').toUpperCase();
  const layout = resolveLayout(recipe.kind, code, name, recipe.sashCount ?? 0);
  const paneStyle: PreviewPaneStyle =
    layout === 'door' ? (recipe.hasGlass ? 'Structure' : 'Solid') : 'Double';

  return {
    layout,
    paneStyle,
    sectorCode: sectorCodeFor(layout),
  };
}

function resolveLayout(
  kind: ProductKind | null | undefined,
  code: string,
  name: string,
  sashCount: number,
): PreviewLayout {
  if (code.includes('TILT') || name.includes('TILT') || name.includes('قلاب')) {
    return 'tilt';
  }
  if (kind === 'Door' || code.includes('DOOR') || name.includes('باب')) {
    return 'door';
  }
  if (
    sashCount >= 2 ||
    code.includes('SLID') ||
    name.includes('SLID') ||
    name.includes('سحاب')
  ) {
    return 'sliding';
  }
  return 'casement';
}

function sectorCodeFor(layout: PreviewLayout): string {
  if (layout === 'door') return 'DOOR';
  if (layout === 'tilt') return 'TILT';
  if (layout === 'sliding') return 'WIN-SLIDING';
  return 'WIN-CASEMENT';
}
