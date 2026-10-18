import {
  AfterViewInit,
  Component,
  ElementRef,
  OnInit,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { MaterialsApi, Material } from '../../../core/api/materials.api';
import { Sector, SectorsApi } from '../../../core/api/sectors.api';
import { StockBar, StockBarsApi } from '../../../core/api/stock-bars.api';
import {
  apiErrorMessage,
  apiFieldErrors,
  firstFieldError,
  isI18nKey,
} from '../../../core/http/api-error';
import { MotionService } from '../../../core/motion/motion.service';
import { FormFieldError } from '../../../shared/form-field/form-field-error';

@Component({
  selector: 'app-materials-page',
  imports: [FormsModule, TranslatePipe, DecimalPipe, FormFieldError],
  templateUrl: './materials-page.html',
  styleUrl: './materials-page.scss',
})
export class MaterialsPage implements OnInit, AfterViewInit {
  private readonly materialsApi = inject(MaterialsApi);
  private readonly sectorsApi = inject(SectorsApi);
  private readonly stockBarsApi = inject(StockBarsApi);
  private readonly motion = inject(MotionService);

  private readonly root = viewChild<ElementRef<HTMLElement>>('pageRoot');

  readonly materials = signal<Material[]>([]);
  readonly sectors = signal<Sector[]>([]);
  readonly stockBars = signal<StockBar[]>([]);
  readonly selectedMaterialId = signal<string | null>(null);
  readonly error = signal<string | null>(null);
  readonly materialFieldErrors = signal<{
    code?: string | null;
    name?: string | null;
    sectorId?: string | null;
    description?: string | null;
  }>({});
  readonly stockFieldErrors = signal<{
    lengthMm?: string | null;
    unitPrice?: string | null;
    availableQuantity?: string | null;
  }>({});
  readonly editingQty = signal<Record<string, number>>({});

  readonly isI18nKey = isI18nKey;

  materialCode = '';
  materialName = '';
  materialSectorId = '';
  materialDescription = '';

  stockLengthMm = 6000;
  stockUnitPrice = 120;
  stockQuantity = 10;

  ngOnInit(): void {
    this.reload();
  }

  ngAfterViewInit(): void {
    this.motion.pageEnter(this.root()?.nativeElement ?? null);
  }

  reload(): void {
    this.sectorsApi.list().subscribe({
      next: (sectors) => {
        this.sectors.set(sectors);
        if (!this.materialSectorId && sectors.length) {
          this.materialSectorId = sectors[0].id;
        }
      },
    });
    this.materialsApi.list().subscribe({
      next: (materials) => {
        this.materials.set(materials);
        const selected =
          this.selectedMaterialId() &&
          materials.some((m) => m.id === this.selectedMaterialId())
            ? this.selectedMaterialId()!
            : (materials[0]?.id ?? null);
        this.selectedMaterialId.set(selected);
        if (selected) this.loadStock(selected);
        else this.stockBars.set([]);
      },
      error: (err) => this.error.set(apiErrorMessage(err, 'common.loadError')),
    });
  }

  selectMaterial(id: string): void {
    this.selectedMaterialId.set(id);
    this.loadStock(id);
  }

  loadStock(materialId: string): void {
    this.stockBarsApi.list({ materialId }).subscribe({
      next: (bars) => {
        this.stockBars.set(bars);
        const drafts: Record<string, number> = {};
        for (const bar of bars) {
          drafts[bar.id] = bar.availableQuantity;
        }
        this.editingQty.set(drafts);
      },
      error: (err) => this.error.set(apiErrorMessage(err, 'common.loadError')),
    });
  }

  setQtyDraft(barId: string, value: string | number): void {
    this.editingQty.update((map) => ({
      ...map,
      [barId]: Number(value),
    }));
  }

  createMaterial(): void {
    this.materialFieldErrors.set({});
    this.error.set(null);

    if (!this.materialCode.trim() || !this.materialName.trim() || !this.materialSectorId) {
      this.materialFieldErrors.set({
        code: !this.materialCode.trim() ? 'materials.validation.codeRequired' : null,
        name: !this.materialName.trim() ? 'materials.validation.nameRequired' : null,
        sectorId: !this.materialSectorId ? 'materials.validation.sectorRequired' : null,
      });
      return;
    }

    this.materialsApi
      .create({
        sectorId: this.materialSectorId,
        code: this.materialCode.trim(),
        name: this.materialName.trim(),
        description: this.materialDescription.trim() || null,
      })
      .subscribe({
        next: () => {
          this.materialCode = '';
          this.materialName = '';
          this.materialDescription = '';
          this.reload();
        },
        error: (err) => {
          const fields = apiFieldErrors(err);
          this.materialFieldErrors.set({
            code: firstFieldError(fields, 'code'),
            name: firstFieldError(fields, 'name'),
            sectorId: firstFieldError(fields, 'sectorId'),
            description: firstFieldError(fields, 'description'),
          });
          this.error.set(apiErrorMessage(err, 'common.saveError'));
        },
      });
  }

  createStock(): void {
    const materialId = this.selectedMaterialId();
    if (!materialId) return;

    this.stockFieldErrors.set({});
    this.error.set(null);

    this.stockBarsApi
      .create({
        materialId,
        lengthMm: Number(this.stockLengthMm),
        unitPrice: Number(this.stockUnitPrice),
        availableQuantity: Number(this.stockQuantity),
      })
      .subscribe({
        next: () => this.loadStock(materialId),
        error: (err) => {
          const fields = apiFieldErrors(err);
          this.stockFieldErrors.set({
            lengthMm: firstFieldError(fields, 'lengthMm'),
            unitPrice: firstFieldError(fields, 'unitPrice'),
            availableQuantity: firstFieldError(fields, 'availableQuantity'),
          });
          this.error.set(apiErrorMessage(err, 'common.saveError'));
        },
      });
  }

  saveStockQty(bar: StockBar): void {
    const qty = this.editingQty()[bar.id];
    if (qty === undefined || Number.isNaN(qty) || qty < 0) return;

    this.stockBarsApi
      .update(bar.id, {
        materialId: bar.materialId,
        lengthMm: bar.lengthMm,
        unitPrice: bar.unitPrice,
        availableQuantity: qty,
      })
      .subscribe({
        next: () => {
          const materialId = this.selectedMaterialId();
          if (materialId) this.loadStock(materialId);
        },
        error: (err) => this.error.set(apiErrorMessage(err, 'common.saveError')),
      });
  }

  deactivate(bar: StockBar): void {
    this.stockBarsApi.deactivate(bar.id).subscribe({
      next: () => {
        const materialId = this.selectedMaterialId();
        if (materialId) this.loadStock(materialId);
      },
      error: (err) => this.error.set(apiErrorMessage(err, 'common.saveError')),
    });
  }
}
