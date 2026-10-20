import { Component, OnInit, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { switchMap, of, map } from 'rxjs';
import {
  AccessoriesApi,
  AccessoryUnit,
} from '../../core/api/accessories.api';
import { GlassApi, GlassType } from '../../core/api/glass.api';
import { MaterialsApi } from '../../core/api/materials.api';
import { SectorsApi } from '../../core/api/sectors.api';
import {
  MARKET_MATERIALS_SECTOR_NAME_AR,
  MARKET_MATERIALS_SECTOR_NAME_EN,
} from '../../core/constants/market';
import { apiErrorMessage } from '../../core/http/api-error';
import { LanguageService } from '../../core/services/language.service';
import { WorkshopContextService } from '../../core/services/workshop-context.service';

export type QuickAddMode = 'aluminum' | 'glassSpec' | 'accessory';

export interface QuickAddResult {
  mode: QuickAddMode;
  id: string;
}

@Component({
  selector: 'app-resource-quick-add',
  imports: [FormsModule, TranslatePipe],
  templateUrl: './resource-quick-add.html',
  styleUrl: './resource-quick-add.scss',
})
export class ResourceQuickAdd implements OnInit {
  private readonly materialsApi = inject(MaterialsApi);
  private readonly accessoriesApi = inject(AccessoriesApi);
  private readonly glassApi = inject(GlassApi);
  private readonly sectorsApi = inject(SectorsApi);
  private readonly workshopContext = inject(WorkshopContextService);
  private readonly language = inject(LanguageService);

  readonly mode = input.required<QuickAddMode>();
  readonly closed = output<void>();
  readonly created = output<QuickAddResult>();

  readonly glassTypes = signal<GlassType[]>([]);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);

  code = '';
  name = '';
  glassTypeId = '';
  newGlassTypeName = '';
  thicknessMm: number | null = 6;
  pricePerSqm: number | null = 100;
  brand = '';
  unit: AccessoryUnit = 'Piece';
  unitPrice: number | null = 50;

  readonly units: AccessoryUnit[] = ['Piece', 'Set', 'Meter'];

  ngOnInit(): void {
    if (this.mode() === 'glassSpec') {
      const workshopId = this.workshopContext.selectedWorkshopId();
      if (!workshopId) return;
      this.glassApi.listTypes(workshopId, true).subscribe({
        next: (types) => {
          this.glassTypes.set(types);
          if (types.length) this.glassTypeId = types[0].id;
        },
        error: () => this.glassTypes.set([]),
      });
    }
  }

  cancel(): void {
    this.closed.emit();
  }

  submit(): void {
    this.error.set(null);
    const mode = this.mode();
    if (mode === 'aluminum') this.createAluminum();
    else if (mode === 'glassSpec') this.createGlass();
    else this.createAccessory();
  }

  private createAluminum(): void {
    if (!this.code.trim() || !this.name.trim()) {
      this.error.set('resources.validation.nameRequired');
      return;
    }
    this.saving.set(true);
    this.ensureMarketSector()
      .pipe(
        switchMap((sector) =>
          this.materialsApi.create({
            sectorId: sector.id,
            code: this.code.trim(),
            name: this.name.trim(),
            category: 'Aluminum',
          }),
        ),
      )
      .subscribe({
        next: (res) => {
          this.saving.set(false);
          this.created.emit({ mode: 'aluminum', id: res.id });
        },
        error: (err) => {
          this.saving.set(false);
          this.error.set(apiErrorMessage(err, 'common.saveError'));
        },
      });
  }

  private createGlass(): void {
    const workshopId = this.workshopContext.selectedWorkshopId();
    if (!workshopId) {
      this.error.set('resources.needWorkshop');
      return;
    }
    const thickness = Number(this.thicknessMm);
    const price = Number(this.pricePerSqm);
    if (!thickness || thickness <= 0 || !price || price <= 0) {
      this.error.set('common.saveError');
      return;
    }

    this.saving.set(true);
    const typeId$ = this.newGlassTypeName.trim()
      ? this.glassApi
          .createType(workshopId, { name: this.newGlassTypeName.trim() })
          .pipe(map((t) => t.id))
      : this.glassTypeId
        ? of(this.glassTypeId)
        : null;

    if (!typeId$) {
      this.saving.set(false);
      this.error.set('common.saveError');
      return;
    }

    typeId$
      .pipe(
        switchMap((typeId) =>
          this.glassApi.createSpec(typeId, {
            thicknessMm: thickness,
            pricePerSqm: price,
          }),
        ),
      )
      .subscribe({
        next: (spec) => {
          this.saving.set(false);
          this.created.emit({ mode: 'glassSpec', id: spec.id });
        },
        error: (err) => {
          this.saving.set(false);
          this.error.set(apiErrorMessage(err, 'common.saveError'));
        },
      });
  }

  private createAccessory(): void {
    const workshopId = this.workshopContext.selectedWorkshopId();
    if (!workshopId) {
      this.error.set('resources.needWorkshop');
      return;
    }
    const name = this.name.trim();
    const unitPrice = Number(this.unitPrice);
    if (!name || !unitPrice || unitPrice <= 0) {
      this.error.set('common.saveError');
      return;
    }
    this.saving.set(true);
    this.accessoriesApi
      .create(workshopId, {
        name,
        unitPrice,
        brand: this.brand.trim() || null,
        unit: this.unit,
      })
      .subscribe({
        next: (item) => {
          this.saving.set(false);
          this.created.emit({ mode: 'accessory', id: item.id });
        },
        error: (err) => {
          this.saving.set(false);
          this.error.set(apiErrorMessage(err, 'common.saveError'));
        },
      });
  }

  private ensureMarketSector() {
    return this.sectorsApi.ensureMarketSector(
      this.language.currentLanguage() === 'ar'
        ? MARKET_MATERIALS_SECTOR_NAME_AR
        : MARKET_MATERIALS_SECTOR_NAME_EN,
    );
  }
}
