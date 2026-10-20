import { Component, computed, inject, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { AluminumFinishService } from '../../core/services/aluminum-finish.service';
import { GlassTintService } from '../../core/services/glass-tint.service';
import { formatCm } from '../../core/units/length';

export type FrameKind = 'window' | 'door' | 'hopper';
/** Visual pane style for preview SVG (not tied to catalog materials). */
export type PreviewPaneStyle = 'Structure' | 'Double' | 'Solid';
/** Joinery layout for recipes try-size (quotes can leave `auto`). */
export type PreviewLayout = 'auto' | 'sliding' | 'casement' | 'tilt' | 'door';

export type PreviewRole =
  | 'FrameTop'
  | 'FrameBottom'
  | 'FrameJamb'
  | 'SashStile'
  | 'SashRail'
  | 'InterlockStile';

export const DEFAULT_ROLE_COLORS: Record<PreviewRole, string> = {
  FrameTop: '#0d9488',
  FrameBottom: '#d97706',
  FrameJamb: '#16a34a',
  SashStile: '#2563eb',
  SashRail: '#7c3aed',
  InterlockStile: '#e11d48',
};

@Component({
  selector: 'app-frame-preview',
  imports: [TranslatePipe],
  templateUrl: './frame-preview.html',
  styleUrl: './frame-preview.scss',
})
export class FramePreview {
  private readonly finishService = inject(AluminumFinishService);
  private readonly glassService = inject(GlassTintService);

  readonly label = input.required<string>();
  readonly sectorCode = input<string>('');
  readonly sectorName = input<string>('');
  readonly heightMm = input<number | null>(null);
  readonly widthMm = input<number | null>(null);
  readonly quantity = input<number | null>(1);
  readonly compact = input(false);
  readonly paneStyle = input<PreviewPaneStyle | null>(null);
  readonly layout = input<PreviewLayout>('auto');
  /** Role colors for recipes try-size only — do not pass from quotes/projects/plans. */
  readonly roleColors = input<Partial<Record<PreviewRole, string>> | null>(null);

  readonly formatCm = formatCm;
  readonly finish = this.finishService.finish;
  readonly glass = this.glassService.tint;

  readonly resolvedLayout = computed<PreviewLayout>(() => {
    const explicit = this.layout();
    if (explicit !== 'auto') return explicit;
    // Heuristic for callers that only pass sector codes (quotes/projects).
    const code = this.sectorCode().toUpperCase();
    const name = `${this.sectorName()} ${this.label()}`.toUpperCase();
    if (code.includes('TILT') || name.includes('TILT') || name.includes('قلاب')) {
      return 'tilt';
    }
    if (code.includes('DOOR') || name.includes('DOOR') || name.includes('باب')) {
      return 'door';
    }
    if (
      code.includes('SLID') ||
      name.includes('SLID') ||
      name.includes('سحاب')
    ) {
      return 'sliding';
    }
    if (
      code.includes('CASE') ||
      name.includes('CASE') ||
      name.includes('مفصلي')
    ) {
      return 'casement';
    }
    return 'sliding';
  });

  readonly kind = computed<FrameKind>(() => {
    const layout = this.resolvedLayout();
    if (layout === 'tilt') return 'hopper';
    if (layout === 'door') return 'door';
    if (layout === 'sliding' || layout === 'casement') return 'window';

    const code = this.sectorCode().toUpperCase();
    const name = this.sectorName().toUpperCase();
    if (
      code.includes('HOPPER') ||
      code === 'HOP' ||
      code.includes('TILT') ||
      name.includes('HOPPER') ||
      name.includes('قلاب')
    ) {
      return 'hopper';
    }
    if (
      code.includes('DOOR') ||
      code === 'DR' ||
      name.includes('DOOR') ||
      name.includes('باب')
    ) {
      return 'door';
    }
    return 'window';
  });

  readonly resolvedPaneStyle = computed<PreviewPaneStyle>(() => {
    const explicit = this.paneStyle();
    if (explicit) return explicit;
    // Safer default: solid aluminum door (not glass) when caller is silent.
    if (this.kind() === 'door') return 'Solid';
    if (this.kind() === 'hopper') return 'Double';
    return 'Double';
  });

  readonly gradientId = computed(
    () =>
      `alum-${this.label().replace(/\W+/g, '')}-${this.kind()}-${this.finish().id}-${this.glass().id}-${this.resolvedPaneStyle()}-${this.resolvedLayout()}`,
  );

  readonly hasSize = computed(() => {
    const h = this.heightMm();
    const w = this.widthMm();
    return !!h && h > 0 && !!w && w > 0;
  });

  readonly showMullion = computed(
    () => this.kind() === 'window' && this.resolvedLayout() === 'sliding',
  );

  readonly hasRoleColors = computed(() => {
    const map = this.roleColors();
    return !!map && Object.keys(map).length > 0;
  });

  readonly geometry = computed(() => {
    const h = this.heightMm() ?? 1000;
    const w = this.widthMm() ?? 1000;
    const max = this.compact() ? 96 : 140;
    const isHopper = this.kind() === 'hopper';

    // --- Hopper: height-scaled side elevation (horizontal = depth, not opening width) ---
    if (isHopper) {
      const drawScale = max / Math.max(h, 1);
      const ih = Math.max(52, Math.min(max + 8, h * drawScale));
      const frame = Math.max(8, Math.min(13, ih * 0.09));
      const frameDepth = 15;
      const topPad = 16;
      const bottomPad = 34;
      const y = topPad;
      const hingeY = y + frame * 0.65;
      const clearH = Math.max(28, ih - frame * 1.15);
      let sashLen = clearH;
      let openDeg = 30;
      // Keep tip inside padded canvas: reduce length then angle if needed.
      for (let i = 0; i < 8; i++) {
        const rad = (openDeg * Math.PI) / 180;
        const tipY = hingeY + Math.cos(rad) * sashLen;
        const tipXOverhang = Math.sin(rad) * sashLen;
        const maxTipY = y + ih + bottomPad - 20;
        if (tipY <= maxTipY && tipXOverhang <= ih * 0.55) break;
        if (tipY > maxTipY) sashLen *= 0.92;
        else openDeg = Math.max(18, openDeg - 2);
      }
      const openRad = (openDeg * Math.PI) / 180;
      const overhang = Math.sin(openRad) * sashLen;
      // Place frame so open tip + handle stay inside the view (horizontal = depth).
      const leftSafe = 10;
      const frameFrontX = leftSafe + overhang + 22;
      const wallX = frameFrontX - 12;
      const hingeX = frameFrontX + 2.5;
      const sashTipX = hingeX - Math.sin(openRad) * sashLen;
      const sashTipY = hingeY + Math.cos(openRad) * sashLen;
      const sashThick = Math.max(3.4, frame * 0.42);
      const nx = -Math.cos(openRad);
      const ny = -Math.sin(openRad);
      const hopperPoints = [
        `${hingeX + nx * sashThick * 0.12},${hingeY + ny * sashThick * 0.12}`,
        `${hingeX - nx * sashThick},${hingeY - ny * sashThick}`,
        `${sashTipX - nx * sashThick},${sashTipY - ny * sashThick}`,
        `${sashTipX + nx * sashThick * 0.12},${sashTipY + ny * sashThick * 0.12}`,
      ].join(' ');
      // Aluminum rail strips along sash long edges (for richer SVG).
      const railT = Math.max(1.6, sashThick * 0.28);
      const hopperRailOuter = [
        `${hingeX - nx * sashThick},${hingeY - ny * sashThick}`,
        `${hingeX - nx * (sashThick - railT)},${hingeY - ny * (sashThick - railT)}`,
        `${sashTipX - nx * (sashThick - railT)},${sashTipY - ny * (sashThick - railT)}`,
        `${sashTipX - nx * sashThick},${sashTipY - ny * sashThick}`,
      ].join(' ');
      const hopperRailInner = [
        `${hingeX + nx * sashThick * 0.12},${hingeY + ny * sashThick * 0.12}`,
        `${hingeX + nx * (sashThick * 0.12 - railT)},${hingeY + ny * (sashThick * 0.12 - railT)}`,
        `${sashTipX + nx * (sashThick * 0.12 - railT)},${sashTipY + ny * (sashThick * 0.12 - railT)}`,
        `${sashTipX + nx * sashThick * 0.12},${sashTipY + ny * sashThick * 0.12}`,
      ].join(' ');
      const closedSashX = hingeX;
      const closedTipY = hingeY + sashLen;
      const arcR = sashLen;
      const hopHandleX = sashTipX - nx * sashThick * 0.45;
      const hopHandleY = sashTipY - ny * sashThick * 0.45;
      const hopS = Math.min(1.4, Math.max(0.85, sashThick / 4));
      const viewW = frameFrontX + frameDepth + 36;
      const viewH = y + ih + bottomPad;
      const iguGap = Math.max(1.2, Math.min(2.2, frame * 0.16));
      const glassMidX1 = hingeX - nx * sashThick * 0.45;
      const glassMidY1 = hingeY - ny * sashThick * 0.45;
      const glassMidX2 = sashTipX - nx * sashThick * 0.45;
      const glassMidY2 = sashTipY - ny * sashThick * 0.45;
      const headLipH = frame * 0.55;
      const sillLipH = frame * 0.5;
      const pocketX = frameFrontX + 2.2;
      const pocketY = y + headLipH;
      const pocketW = Math.max(4, frameDepth - 4.4);
      const pocketH = Math.max(12, ih - headLipH - sillLipH);
      const lipX = frameFrontX - 1.5;
      const lipW = frameDepth + 3;
      const sillLipY = y + ih - sillLipH;

      return {
        viewW,
        viewH,
        x: frameFrontX,
        y,
        w: frameDepth,
        h: ih,
        frame,
        innerX: frameFrontX + 2,
        innerY: hingeY,
        innerW: frameDepth - 4,
        innerH: clearH,
        midX: frameFrontX + frameDepth / 2,
        midY: y + ih / 2,
        widthLabelX: frameFrontX + frameDepth / 2,
        widthLabelY: y + ih + 18,
        heightLabelX: 10,
        heightLabelY: y + ih / 2,
        pocketX,
        pocketY,
        pocketW,
        pocketH,
        lipX,
        lipW,
        headLipH,
        sillLipH,
        sillLipY,
        winBaseW: 0,
        winBaseH: 0,
        winLeverW: 0,
        winLeverH: 0,
        winLeverX: 0,
        doorKnobR: 0,
        doorLeverW: 0,
        doorLeverH: 0,
        doorLeverX: 0,
        handleX: 0,
        handleY: 0,
        mullionW: 0,
        iguGap,
        hopperPoints,
        hopperRailOuter,
        hopperRailInner,
        hingeY,
        hingeX,
        wallX,
        frameFrontX,
        frameDepth,
        frameBackX: frameFrontX + frameDepth,
        sillY: y + ih,
        closedSashX,
        closedTipY,
        sashTipX,
        sashTipY,
        arcR,
        hopHandleX,
        hopHandleY,
        hopHandleR: Math.max(2.2, 2.5 * hopS),
        hopLeverW: Math.max(6.5, 7.5 * hopS),
        hopLeverH: Math.max(1.8, 2.1 * hopS),
        hopHingeR: Math.max(2.2, 2.4 * hopS),
        glassMidX1,
        glassMidY1,
        glassMidX2,
        glassMidY2,
        sashX: 0,
        sashY: 0,
        sashW: 0,
        sashH: 0,
        sashStroke: 0,
      };
    }

    // --- Front elevation (sliding / casement / door) ---
    const drawScale = max / Math.max(h, w);
    const ih = Math.max(40, h * drawScale);
    const iw = Math.max(40, w * drawScale);
    const sidePad = 28;
    const topPad = 14;
    const bottomPad = 28;
    const frame = Math.max(7, Math.min(14, Math.min(iw, ih) * 0.1));
    const x = sidePad;
    const y = topPad;
    const innerX = x + frame;
    const innerY = y + frame;
    const innerW = Math.max(6, iw - frame * 2);
    const innerH = Math.max(6, ih - frame * 2);
    const midX = x + iw / 2;
    const midY = y + ih / 2;

    const twoPane = this.resolvedLayout() === 'sliding';

    const handleScale = Math.min(1.6, Math.max(0.75, Math.min(innerW, innerH) / 80));
    const inset = Math.min(innerW * 0.22, Math.max(frame * 0.85, 5 * handleScale));
    const handleX = innerX + innerW - inset;
    const handleY = midY;
    const s = handleScale;

    const sashInset = Math.max(2.5, frame * 0.35);
    const sashX = twoPane ? midX + sashInset * 0.35 : innerX + sashInset;
    const sashY = innerY + sashInset;
    const sashW = twoPane
      ? Math.max(4, innerX + innerW - sashInset - (midX + sashInset * 0.35))
      : Math.max(4, innerW - sashInset * 2);
    const sashH = Math.max(4, innerH - sashInset * 2);
    const sashStroke = Math.max(2.2, frame * 0.28);

    return {
      viewW: iw + sidePad * 2,
      viewH: ih + topPad + bottomPad,
      x,
      y,
      w: iw,
      h: ih,
      frame,
      innerX,
      innerY,
      innerW,
      innerH,
      midX,
      midY,
      widthLabelX: x + iw / 2,
      widthLabelY: y + ih + 16,
      heightLabelX: 12,
      heightLabelY: y + ih / 2,
      winBaseW: 3.2 * s,
      winBaseH: 11 * s,
      winLeverW: 7.5 * s,
      winLeverH: 2.2 * s,
      winLeverX: -(3.2 * s) / 2 - 7.5 * s,
      doorKnobR: 2.8 * s,
      doorLeverW: 11 * s,
      doorLeverH: 2.6 * s,
      doorLeverX: -(2.8 * s) - 11 * s + 0.4 * s,
      handleX,
      handleY,
      mullionW: Math.max(2.4, Math.min(4.2, frame * 0.45)),
      iguGap: Math.max(1.2, Math.min(2.4, frame * 0.18)),
      hopperPoints: '',
      hopperRailOuter: '',
      hopperRailInner: '',
      hingeY: 0,
      hingeX: 0,
      wallX: 0,
      frameFrontX: 0,
      frameDepth: 0,
      frameBackX: 0,
      sillY: 0,
      closedSashX: 0,
      closedTipY: 0,
      sashTipX: 0,
      sashTipY: 0,
      arcR: 0,
      hopHandleX: 0,
      hopHandleY: 0,
      hopHandleR: 0,
      hopLeverW: 0,
      hopLeverH: 0,
      hopHingeR: 0,
      glassMidX1: 0,
      glassMidY1: 0,
      glassMidX2: 0,
      glassMidY2: 0,
      pocketX: 0,
      pocketY: 0,
      pocketW: 0,
      pocketH: 0,
      lipX: 0,
      lipW: 0,
      headLipH: 0,
      sillLipH: 0,
      sillLipY: 0,
      sashX,
      sashY,
      sashW,
      sashH,
      sashStroke,
    };
  });

  readonly qty = computed(() => Math.max(1, this.quantity() ?? 1));

  readonly isStructure = computed(() => this.resolvedPaneStyle() === 'Structure');
  readonly isDouble = computed(() => this.resolvedPaneStyle() === 'Double');
  readonly isSolid = computed(() => this.resolvedPaneStyle() === 'Solid');

  readonly glassStops = computed(() => {
    const g = this.glass();
    const structure = this.isStructure();
    const boost = structure ? 1.2 : 1;
    return {
      topOpacity: Math.min(1, g.topOpacity * boost),
      midOpacity: Math.min(1, g.midOpacity * boost),
      bottomOpacity: Math.min(1, g.bottomOpacity * boost),
    };
  });

  roleColor(role: PreviewRole): string | null {
    const map = this.roleColors();
    if (!map) return null;
    return map[role] ?? DEFAULT_ROLE_COLORS[role] ?? null;
  }
}
