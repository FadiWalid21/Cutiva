import { Component, computed, inject, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { AluminumFinishService } from '../../core/services/aluminum-finish.service';
import { GlassTintService } from '../../core/services/glass-tint.service';
import { formatCm } from '../../core/units/length';

export type FrameKind = 'window' | 'door' | 'hopper';
/** Visual pane style for preview SVG (not tied to catalog materials). */
export type PreviewPaneStyle = 'Structure' | 'Double';

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

  readonly formatCm = formatCm;
  readonly finish = this.finishService.finish;
  readonly glass = this.glassService.tint;

  readonly resolvedPaneStyle = computed<PreviewPaneStyle>(() => {
    const explicit = this.paneStyle();
    if (explicit) return explicit;
    if (this.kind() === 'door') return 'Structure';
    if (this.kind() === 'hopper') return 'Double';
    return 'Double';
  });

  readonly gradientId = computed(
    () =>
      `alum-${this.label().replace(/\W+/g, '')}-${this.kind()}-${this.finish().id}-${this.glass().id}-${this.resolvedPaneStyle()}`,
  );

  readonly hasSize = computed(() => {
    const h = this.heightMm();
    const w = this.widthMm();
    return !!h && h > 0 && !!w && w > 0;
  });

  readonly kind = computed<FrameKind>(() => {
    const code = this.sectorCode().toUpperCase();
    const name = this.sectorName().toUpperCase();
    if (
      code.includes('HOPPER') ||
      code === 'HOP' ||
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

  readonly geometry = computed(() => {
    const h = this.heightMm() ?? 1000;
    const w = this.widthMm() ?? 1000;
    const max = this.compact() ? 96 : 140;
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

    const handleScale = Math.min(1.6, Math.max(0.75, Math.min(innerW, innerH) / 80));
    const inset = Math.min(innerW * 0.22, Math.max(frame * 0.85, 5 * handleScale));
    const handleX = innerX + innerW - inset;
    const handleY = midY;
    const s = handleScale;

    // Hopper: sash tilts out from bottom (awning); hinge along top inner edge
    const tilt = Math.min(innerH * 0.28, 22);
    const sashBottomY = innerY + innerH - 2;
    const sashTopY = innerY + 2;
    const hopperPoints = [
      `${innerX + 2},${sashTopY}`,
      `${innerX + innerW - 2},${sashTopY}`,
      `${innerX + innerW - 6},${sashBottomY - tilt}`,
      `${innerX + 6},${sashBottomY - tilt}`,
    ].join(' ');

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
      hopperPoints,
      hingeY: sashTopY,
      hingeX1: innerX + 4,
      hingeX2: innerX + innerW - 4,
    };
  });

  readonly qty = computed(() => Math.max(1, this.quantity() ?? 1));

  readonly isStructure = computed(() => this.resolvedPaneStyle() === 'Structure');
  readonly isDouble = computed(() => this.resolvedPaneStyle() === 'Double');

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
}

