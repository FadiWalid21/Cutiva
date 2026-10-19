import { Component, computed, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { formatCm } from '../../../../core/units/length';
import { RemnantInventoryItem } from '../../data/dashboard.models';

interface RemnantChip {
  key: string;
  materialName: string;
  lengthMm: number;
  quantity: number;
}

const MAX_VISIBLE = 12;

@Component({
  selector: 'app-remnant-panel',
  imports: [TranslatePipe],
  templateUrl: './remnant-panel.html',
  styleUrl: './remnant-panel.scss',
})
export class RemnantPanel {
  readonly remnants = input.required<RemnantInventoryItem[]>();

  readonly formatCm = formatCm;

  readonly chips = computed(() => {
    const grouped = new Map<string, RemnantChip>();

    for (const item of this.remnants()) {
      const key = `${item.materialId}:${item.lengthMm}`;
      const existing = grouped.get(key);
      if (existing) {
        existing.quantity += item.quantity;
      } else {
        grouped.set(key, {
          key,
          materialName: item.materialName,
          lengthMm: item.lengthMm,
          quantity: item.quantity,
        });
      }
    }

    return [...grouped.values()].sort(
      (a, b) => b.lengthMm - a.lengthMm || a.materialName.localeCompare(b.materialName),
    );
  });

  readonly visibleChips = computed(() => this.chips().slice(0, MAX_VISIBLE));

  readonly extraCount = computed(() =>
    Math.max(0, this.chips().length - MAX_VISIBLE),
  );
}
