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
import {
  AccessoriesApi,
  Accessory,
  AccessoryUnit,
} from '../../../core/api/accessories.api';
import { GlassApi, GlassSpec, GlassType } from '../../../core/api/glass.api';
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

type ResourcesTab = 'aluminum' | 'glass' | 'accessories';

@Component({
  selector: 'app-resources-page',
  imports: [FormsModule, TranslatePipe, DecimalPipe, FormFieldError],
  templateUrl: './resources-page.html',
  styleUrl: './resources-page.scss',
})
export class ResourcesPage implements OnInit, AfterViewInit {
  private readonly materialsApi = inject(MaterialsApi);
  private readonly sectorsApi = inject(SectorsApi);
  private readonly stockBarsApi = inject(StockBarsApi);
  private readonly glassApi = inject(GlassApi);
  private readonly accessoriesApi = inject(AccessoriesApi);
  readonly workshopContext = inject(WorkshopContextService);
  private readonly motion = inject(MotionService);
  private readonly language = inject(LanguageService);
  private readonly translate = inject(TranslateService);

  private readonly root = viewChild<ElementRef<HTMLElement>>('pageRoot');

  readonly tab = signal<ResourcesTab>('aluminum');
  readonly materials = signal<Material[]>([]);
  readonly stockBars = signal<StockBar[]>([]);
  readonly glassTypes = signal<GlassType[]>([]);
  readonly accessories = signal<Accessory[]>([]);
  readonly selectedMaterialId = signal<string | null>(null);
  readonly selectedGlassTypeId = signal<string | null>(null);
  readonly selectedMaterial = computed(
    () => this.materials().find((m) => m.id === this.selectedMaterialId()) ?? null,
  );
  readonly selectedGlassType = computed(
    () =>
      this.glassTypes().find((t) => t.id === this.selectedGlassTypeId()) ?? null,
  );
  readonly error = signal<string | null>(null);
  readonly materialFieldErrors = signal<{
    code?: string | null;
    name?: string | null;
  }>({});
  readonly stockFieldErrors = signal<{
    lengthMm?: string | null;
    unitPrice?: string | null;
  }>({});

  readonly isI18nKey = isI18nKey;
  readonly formatCm = formatCm;
  readonly units: AccessoryUnit[] = ['Piece', 'Set', 'Meter'];

  materialCode = '';
  materialName = '';
  materialDescription = '';
  materialThicknessMm: number | null = null;

  stockLengthCm = 600;
  stockUnitPrice = 120;

  glassTypeName = '';
  specThicknessMm = 6;
  specPricePerSqm = 100;
  specDescription = '';

  accessoryName = '';
  accessoryBrand = '';
  accessoryUnit: AccessoryUnit = 'Piece';
  accessoryUnitPrice = 50;
  editingAccessoryId: string | null = null;

  private marketSectorId: string | null = null;

  constructor() {
    effect(() => {
      const workshopId = this.workshopContext.selectedWorkshopId();
      const loaded = this.workshopContext.loaded();
      if (!loaded) return;
      if (!workshopId) {
        this.glassTypes.set([]);
        this.accessories.set([]);
        return;
      }
      if (this.tab() === 'glass') this.reloadGlass(workshopId);
      if (this.tab() === 'accessories') this.reloadAccessories(workshopId);
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

    effect(() => {
      const list = this.glassTypes();
      const selected = this.selectedGlassTypeId();
      if (selected && list.some((t) => t.id === selected)) return;
      this.selectedGlassTypeId.set(list[0]?.id ?? null);
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

  setTab(tab: ResourcesTab): void {
    this.tab.set(tab);
    this.error.set(null);
    const workshopId = this.workshopContext.selectedWorkshopId();
    if (tab === 'aluminum') this.reloadMaterials();
    if (tab === 'glass' && workshopId) this.reloadGlass(workshopId);
    if (tab === 'accessories' && workshopId) this.reloadAccessories(workshopId);
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
    this.materialsApi
      .list({ category: 'Aluminum', activeOnly: true })
      .subscribe({
        next: (materials) => this.materials.set(materials),
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
        code: !this.materialCode.trim()
          ? 'resources.validation.codeRequired'
          : null,
        name: !this.materialName.trim()
          ? 'resources.validation.nameRequired'
          : null,
      });
      return;
    }

    const createWithSector = (sectorId: string) => {
      const thickness =
        this.materialThicknessMm != null && this.materialThicknessMm > 0
          ? Number(this.materialThicknessMm)
          : null;

      this.materialsApi
        .create({
          sectorId,
          code: this.materialCode.trim(),
          name: this.materialName.trim(),
          description: this.materialDescription.trim() || null,
          category: 'Aluminum',
          thicknessMm: thickness,
        })
        .subscribe({
          next: () => {
            this.materialCode = '';
            this.materialName = '';
            this.materialDescription = '';
            this.materialThicknessMm = null;
            this.reloadMaterials();
          },
          error: (err) => {
            const fields = apiFieldErrors(err);
            this.materialFieldErrors.set({
              code: firstFieldError(fields, 'code'),
              name: firstFieldError(fields, 'name'),
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
    if (!confirm(this.translate.instant('resources.deleteBarConfirm'))) return;
    this.stockBarsApi.delete(bar.id).subscribe({
      next: () => {
        const materialId = this.selectedMaterialId();
        if (materialId) this.loadStock(materialId);
      },
      error: (err) => this.error.set(apiErrorMessage(err, 'common.saveError')),
    });
  }

  deleteMaterial(material: Material, event: Event): void {
    event.stopPropagation();
    if (!confirm(this.translate.instant('resources.aluminum.deleteConfirm'))) {
      return;
    }
    this.error.set(null);
    this.materialsApi.delete(material.id).subscribe({
      next: () => {
        if (this.selectedMaterialId() === material.id) {
          this.selectedMaterialId.set(null);
          this.stockBars.set([]);
        }
        this.reloadMaterials();
      },
      error: (err) => this.error.set(apiErrorMessage(err, 'common.deleteError')),
    });
  }

  reloadGlass(workshopId: string): void {
    this.glassApi.listTypes(workshopId, false).subscribe({
      next: (types) => this.glassTypes.set(types),
      error: (err) => this.error.set(apiErrorMessage(err, 'common.loadError')),
    });
  }

  selectGlassType(id: string): void {
    this.selectedGlassTypeId.set(id);
  }

  createGlassType(): void {
    this.error.set(null);
    const workshopId = this.workshopContext.selectedWorkshopId();
    if (!workshopId) {
      this.error.set('resources.needWorkshop');
      return;
    }
    const name = this.glassTypeName.trim();
    if (!name) {
      this.error.set('resources.validation.nameRequired');
      return;
    }

    this.glassApi.createType(workshopId, { name }).subscribe({
      next: (created) => {
        this.glassTypeName = '';
        this.reloadGlass(workshopId);
        this.selectedGlassTypeId.set(created.id);
      },
      error: (err) => this.error.set(apiErrorMessage(err, 'common.saveError')),
    });
  }

  createGlassSpec(): void {
    this.error.set(null);
    const typeId = this.selectedGlassTypeId();
    const workshopId = this.workshopContext.selectedWorkshopId();
    if (!typeId || !workshopId) return;

    const thickness = Number(this.specThicknessMm);
    const price = Number(this.specPricePerSqm);
    if (!thickness || thickness <= 0 || price < 0) {
      this.error.set('common.saveError');
      return;
    }

    this.glassApi
      .createSpec(typeId, {
        thicknessMm: thickness,
        pricePerSqm: price,
        description: this.specDescription.trim() || null,
      })
      .subscribe({
        next: () => {
          this.specThicknessMm = 6;
          this.specPricePerSqm = 100;
          this.specDescription = '';
          this.reloadGlass(workshopId);
        },
        error: (err) => this.error.set(apiErrorMessage(err, 'common.saveError')),
      });
  }

  toggleGlassTypeActive(type: GlassType): void {
    this.glassApi
      .updateType(type.id, { name: type.name, isActive: !type.isActive })
      .subscribe({
        next: () => {
          const workshopId = this.workshopContext.selectedWorkshopId();
          if (workshopId) this.reloadGlass(workshopId);
        },
        error: (err) => this.error.set(apiErrorMessage(err, 'common.saveError')),
      });
  }

  toggleSpecActive(spec: GlassSpec): void {
    this.glassApi
      .updateSpec(spec.id, {
        thicknessMm: spec.thicknessMm,
        pricePerSqm: spec.pricePerSqm,
        description: spec.description,
        isActive: !spec.isActive,
      })
      .subscribe({
        next: () => {
          const workshopId = this.workshopContext.selectedWorkshopId();
          if (workshopId) this.reloadGlass(workshopId);
        },
        error: (err) => this.error.set(apiErrorMessage(err, 'common.saveError')),
      });
  }

  reloadAccessories(workshopId: string): void {
    this.accessoriesApi.listForWorkshop(workshopId, false).subscribe({
      next: (items) => this.accessories.set(items),
      error: (err) => this.error.set(apiErrorMessage(err, 'common.loadError')),
    });
  }

  startEditAccessory(item: Accessory): void {
    this.editingAccessoryId = item.id;
    this.accessoryName = item.name;
    this.accessoryBrand = item.brand ?? '';
    this.accessoryUnit = item.unit ?? 'Piece';
    this.accessoryUnitPrice = item.unitPrice;
  }

  cancelEditAccessory(): void {
    this.editingAccessoryId = null;
    this.accessoryName = '';
    this.accessoryBrand = '';
    this.accessoryUnit = 'Piece';
    this.accessoryUnitPrice = 50;
  }

  saveAccessory(): void {
    this.error.set(null);
    const workshopId = this.workshopContext.selectedWorkshopId();
    if (!workshopId) {
      this.error.set('resources.needWorkshop');
      return;
    }

    const name = this.accessoryName.trim();
    const unitPrice = Number(this.accessoryUnitPrice);
    if (!name || !unitPrice || unitPrice <= 0) {
      this.error.set('common.saveError');
      return;
    }

    const brand = this.accessoryBrand.trim() || null;
    const body = {
      name,
      unitPrice,
      brand,
      unit: this.accessoryUnit,
    };

    const editingId = this.editingAccessoryId;
    if (editingId) {
      this.accessoriesApi
        .update(editingId, { ...body, isActive: true })
        .subscribe({
          next: () => {
            this.cancelEditAccessory();
            this.reloadAccessories(workshopId);
          },
          error: (err) =>
            this.error.set(apiErrorMessage(err, 'common.saveError')),
        });
      return;
    }

    this.accessoriesApi.create(workshopId, body).subscribe({
      next: () => {
        this.cancelEditAccessory();
        this.reloadAccessories(workshopId);
      },
      error: (err) => this.error.set(apiErrorMessage(err, 'common.saveError')),
    });
  }

  deactivateAccessory(item: Accessory): void {
    if (!confirm(this.translate.instant('resources.deactivateAccessoryConfirm')))
      return;
    const workshopId = this.workshopContext.selectedWorkshopId();
    this.accessoriesApi
      .update(item.id, {
        name: item.name,
        unitPrice: item.unitPrice,
        brand: item.brand,
        unit: item.unit,
        isActive: false,
      })
      .subscribe({
        next: () => {
          if (workshopId) this.reloadAccessories(workshopId);
        },
        error: (err) => this.error.set(apiErrorMessage(err, 'common.saveError')),
      });
  }

  reactivateAccessory(item: Accessory): void {
    const workshopId = this.workshopContext.selectedWorkshopId();
    this.accessoriesApi
      .update(item.id, {
        name: item.name,
        unitPrice: item.unitPrice,
        brand: item.brand,
        unit: item.unit,
        isActive: true,
      })
      .subscribe({
        next: () => {
          if (workshopId) this.reloadAccessories(workshopId);
        },
        error: (err) => this.error.set(apiErrorMessage(err, 'common.saveError')),
      });
  }

  unitLabel(unit: AccessoryUnit): string {
    return this.translate.instant(`resources.units.${unit}`);
  }
}
