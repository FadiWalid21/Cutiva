import {
  AfterViewInit,
  Component,
  ElementRef,
  OnInit,
  computed,
  effect,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { Observable, map, of, switchMap } from 'rxjs';
import { AccessoriesApi, Accessory } from '../../../core/api/accessories.api';
import { Material, MaterialsApi } from '../../../core/api/materials.api';
import { Sector, SectorsApi } from '../../../core/api/sectors.api';
import { StockBar, StockBarsApi } from '../../../core/api/stock-bars.api';
import {
  MARKET_MATERIALS_SECTOR_CODE,
  MARKET_MATERIALS_SECTOR_NAME_AR,
  MARKET_MATERIALS_SECTOR_NAME_EN,
} from '../../../core/constants/market';
import {
  apiErrorMessage,
  apiFieldErrors,
  firstFieldError,
  isI18nKey,
} from '../../../core/http/api-error';
import { MotionService } from '../../../core/motion/motion.service';
import { LanguageService } from '../../../core/services/language.service';
import { WorkshopContextService } from '../../../core/services/workshop-context.service';
import { MARKET_UNLIMITED_QTY, cmToMm, formatCm } from '../../../core/units/length';
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
  private readonly accessoriesApi = inject(AccessoriesApi);
  readonly workshopContext = inject(WorkshopContextService);
  private readonly motion = inject(MotionService);
  private readonly language = inject(LanguageService);
  private readonly translate = inject(TranslateService);

  private readonly root = viewChild<ElementRef<HTMLElement>>('pageRoot');

  readonly materials = signal<Material[]>([]);
  readonly stockBars = signal<StockBar[]>([]);
  readonly accessories = signal<Accessory[]>([]);
  readonly selectedMaterialId = signal<string | null>(null);
  readonly selectedMaterial = computed(
    () => this.materials().find((m) => m.id === this.selectedMaterialId()) ?? null,
  );
  readonly error = signal<string | null>(null);
  readonly materialFieldErrors = signal<{
    code?: string | null;
    name?: string | null;
    description?: string | null;
  }>({});
  readonly stockFieldErrors = signal<{
    lengthMm?: string | null;
    unitPrice?: string | null;
  }>({});

  readonly isI18nKey = isI18nKey;
  readonly formatCm = formatCm;

  materialCode = '';
  materialName = '';
  materialDescription = '';

  stockLengthCm = 600;
  stockUnitPrice = 120;

  accessoryName = '';
  accessoryUnitPrice = 50;

  private marketSectorId: string | null = null;

  constructor() {
    effect(() => {
      const workshopId = this.workshopContext.selectedWorkshopId();
      const loaded = this.workshopContext.loaded();
      if (!loaded) return;
      if (!workshopId) {
        this.accessories.set([]);
        return;
      }
      this.reloadAccessories(workshopId);
    });

    effect(() => {
      const list = this.materials();
      const selected = this.selectedMaterialId();
      if (selected && list.some((m) => m.id === selected)) return;
      const next = list[0]?.id ?? null;
      this.selectedMaterialId.set(next);
      if (next) this.loadStock(next);
      else this.stockBars.set([]);
    });
  }

  ngOnInit(): void {
    this.ensureMarketSector().subscribe({
      next: (sector) => {
        this.marketSectorId = sector.id;
        this.reloadMaterials();
      },
      error: (err) => this.error.set(apiErrorMessage(err, 'common.loadError')),
    });
  }

  ngAfterViewInit(): void {
    this.motion.pageEnter(this.root()?.nativeElement ?? null);
  }

  private ensureMarketSector(): Observable<Sector> {
    return this.sectorsApi.list().pipe(
      switchMap((sectors) => {
        const existing = sectors.find(
          (s) => s.code.toUpperCase() === MARKET_MATERIALS_SECTOR_CODE,
        );
        if (existing) return of(existing);

        const name =
          this.language.currentLanguage() === 'ar'
            ? MARKET_MATERIALS_SECTOR_NAME_AR
            : MARKET_MATERIALS_SECTOR_NAME_EN;

        return this.sectorsApi
          .create({
            code: MARKET_MATERIALS_SECTOR_CODE,
            name,
            materialId: null,
          })
          .pipe(
            map(
              (created): Sector => ({
                id: created.id,
                code: created.code,
                name: created.name,
                materialId: created.materialId,
                materialCode: null,
                materialName: null,
              }),
            ),
          );
      }),
    );
  }

  reloadMaterials(): void {
    this.materialsApi.list().subscribe({
      next: (materials) => {
        this.materials.set(materials);
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
      next: (bars) => this.stockBars.set(bars),
      error: (err) => this.error.set(apiErrorMessage(err, 'common.loadError')),
    });
  }

  createMaterial(): void {
    this.materialFieldErrors.set({});
    this.error.set(null);

    if (!this.materialCode.trim() || !this.materialName.trim()) {
      this.materialFieldErrors.set({
        code: !this.materialCode.trim() ? 'materials.validation.codeRequired' : null,
        name: !this.materialName.trim() ? 'materials.validation.nameRequired' : null,
      });
      return;
    }

    const createWithSector = (sectorId: string) => {
      this.materialsApi
        .create({
          sectorId,
          code: this.materialCode.trim(),
          name: this.materialName.trim(),
          description: this.materialDescription.trim() || null,
        })
        .subscribe({
          next: () => {
            this.materialCode = '';
            this.materialName = '';
            this.materialDescription = '';
            this.reloadMaterials();
          },
          error: (err) => {
            const fields = apiFieldErrors(err);
            this.materialFieldErrors.set({
              code: firstFieldError(fields, 'code'),
              name: firstFieldError(fields, 'name'),
              description: firstFieldError(fields, 'description'),
            });
            this.error.set(apiErrorMessage(err, 'common.saveError'));
          },
        });
    };

    if (this.marketSectorId) {
      createWithSector(this.marketSectorId);
      return;
    }

    this.ensureMarketSector().subscribe({
      next: (sector) => {
        this.marketSectorId = sector.id;
        createWithSector(sector.id);
      },
      error: (err) => this.error.set(apiErrorMessage(err, 'common.saveError')),
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
        lengthMm: cmToMm(Number(this.stockLengthCm)),
        unitPrice: Number(this.stockUnitPrice),
        availableQuantity: MARKET_UNLIMITED_QTY,
      })
      .subscribe({
        next: () => this.loadStock(materialId),
        error: (err) => {
          const fields = apiFieldErrors(err);
          this.stockFieldErrors.set({
            lengthMm: firstFieldError(fields, 'lengthMm'),
            unitPrice: firstFieldError(fields, 'unitPrice'),
          });
          this.error.set(apiErrorMessage(err, 'common.saveError'));
        },
      });
  }

  toggleAvailability(bar: StockBar, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    this.stockBarsApi.setAvailability(bar.id, checked).subscribe({
      next: () => {
        const materialId = this.selectedMaterialId();
        if (materialId) this.loadStock(materialId);
      },
      error: (err) => {
        (event.target as HTMLInputElement).checked = bar.isActive;
        this.error.set(apiErrorMessage(err, 'common.saveError'));
      },
    });
  }

  deleteBar(bar: StockBar): void {
    if (!confirm(this.translate.instant('materials.deleteConfirm'))) return;

    this.stockBarsApi.delete(bar.id).subscribe({
      next: () => {
        const materialId = this.selectedMaterialId();
        if (materialId) this.loadStock(materialId);
      },
      error: (err) => this.error.set(apiErrorMessage(err, 'common.saveError')),
    });
  }

  reloadAccessories(workshopId: string): void {
    this.accessoriesApi.listForWorkshop(workshopId).subscribe({
      next: (items) => this.accessories.set(items),
      error: (err) => this.error.set(apiErrorMessage(err, 'common.loadError')),
    });
  }

  createAccessory(): void {
    this.error.set(null);
    const workshopId = this.workshopContext.selectedWorkshopId();
    if (!workshopId) {
      this.error.set('accessories.needWorkshop');
      return;
    }

    const name = this.accessoryName.trim();
    const unitPrice = Number(this.accessoryUnitPrice);
    if (!name || !unitPrice || unitPrice <= 0) {
      this.error.set('common.saveError');
      return;
    }

    this.accessoriesApi.create(workshopId, { name, unitPrice }).subscribe({
      next: () => {
        this.accessoryName = '';
        this.accessoryUnitPrice = 50;
        this.reloadAccessories(workshopId);
      },
      error: (err) => this.error.set(apiErrorMessage(err, 'common.saveError')),
    });
  }

  deleteAccessory(item: Accessory): void {
    if (!confirm(this.translate.instant('accessories.deleteConfirm'))) return;
    const workshopId = this.workshopContext.selectedWorkshopId();
    this.accessoriesApi.delete(item.id).subscribe({
      next: () => {
        if (workshopId) this.reloadAccessories(workshopId);
      },
      error: (err) => this.error.set(apiErrorMessage(err, 'common.saveError')),
    });
  }
}
