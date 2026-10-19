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
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { AccessoriesApi, Accessory } from '../../../core/api/accessories.api';
import { Material, MaterialsApi } from '../../../core/api/materials.api';
import {
  AccessoryQuantityRule,
  ProductTemplatesApi,
  ProductType,
  ProductVariant,
  TemplateAccessorySlot,
  TemplateGlassSlot,
  TemplatePart,
} from '../../../core/api/product-templates.api';
import { apiErrorMessage, isI18nKey } from '../../../core/http/api-error';
import { MotionService } from '../../../core/motion/motion.service';
import { AuthService } from '../../../core/services/auth.service';
import { WorkshopContextService } from '../../../core/services/workshop-context.service';
import {
  FormulaAxis,
  FormulaOp,
  SimpleFormula,
  applySimpleFormula,
  computeLengthMm,
  toSimpleFormula,
} from '../../../core/units/formula-sentence';
import { cmToMm, formatCm, mmToCm } from '../../../core/units/length';
import {
  QuickAddMode,
  QuickAddResult,
  ResourceQuickAdd,
} from '../../../shared/resource-quick-add/resource-quick-add';

@Component({
  selector: 'app-templates-page',
  imports: [FormsModule, TranslatePipe, ResourceQuickAdd],
  templateUrl: './templates-page.html',
  styleUrl: './templates-page.scss',
})
export class TemplatesPage implements OnInit, AfterViewInit {
  private readonly templatesApi = inject(ProductTemplatesApi);
  private readonly materialsApi = inject(MaterialsApi);
  private readonly accessoriesApi = inject(AccessoriesApi);
  private readonly workshopContext = inject(WorkshopContextService);
  private readonly motion = inject(MotionService);
  private readonly auth = inject(AuthService);

  private readonly root = viewChild<ElementRef<HTMLElement>>('pageRoot');

  readonly types = signal<ProductType[]>([]);
  readonly selectedTypeId = signal<string | null>(null);
  readonly selectedVariantId = signal<string | null>(null);
  readonly variant = signal<ProductVariant | null>(null);
  readonly aluminumMaterials = signal<Material[]>([]);
  readonly accessories = signal<Accessory[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly message = signal<string | null>(null);
  readonly canManageCatalog = this.auth.canManageCatalog;

  readonly quickAddMode = signal<QuickAddMode | null>(null);
  readonly quickAddTargetPartId = signal<string | null>(null);
  readonly quickAddTargetSlotId = signal<string | null>(null);

  readonly isI18nKey = isI18nKey;
  readonly quantityRules: AccessoryQuantityRule[] = ['PerProduct', 'PerSash'];
  readonly axes: FormulaAxis[] = ['width', 'height'];
  readonly ops: FormulaOp[] = ['plus', 'minus'];

  /** Live preview inputs (cm). */
  readonly previewWidthCm = signal(100);
  readonly previewHeightCm = signal(120);

  readonly selectedType = computed(
    () => this.types().find((t) => t.id === this.selectedTypeId()) ?? null,
  );

  readonly previewRows = computed(() => {
    const v = this.variant();
    if (!v) return [];
    const wMm = cmToMm(this.previewWidthCm());
    const hMm = cmToMm(this.previewHeightCm());
    return v.parts.map((part) => {
      const lengthMm = computeLengthMm({
        widthMm: wMm,
        heightMm: hMm,
        widthCoef: part.widthCoef,
        heightCoef: part.heightCoef,
        offsetMm: part.offsetMm,
        useSashWidthDivision: part.useSashWidthDivision,
        sashCount: v.sashCount,
        overlapMm: v.overlapMm,
      });
      return {
        id: part.id,
        name: part.displayName,
        qty: part.qtyPerUnit,
        lengthCm: formatCm(Math.round(lengthMm)),
      };
    });
  });

  // Editable draft fields for variant header (overlap in cm in UI)
  variantName = '';
  variantSashCount: number | null = null;
  variantOverlapCm: number | null = null;
  variantIsActive = true;

  constructor() {
    effect(() => {
      const workshopId = this.workshopContext.selectedWorkshopId();
      const loaded = this.workshopContext.loaded();
      if (!loaded || !workshopId) {
        this.accessories.set([]);
        return;
      }
      this.accessoriesApi.listForWorkshop(workshopId, true).subscribe({
        next: (items) => this.accessories.set(items),
        error: () => this.accessories.set([]),
      });
    });
  }

  ngOnInit(): void {
    this.reloadTypes();
    this.reloadAluminum();
  }

  ngAfterViewInit(): void {
    this.motion.pageEnter(this.root()?.nativeElement ?? null);
  }

  reloadAluminum(): void {
    this.materialsApi.list({ category: 'Aluminum', activeOnly: true }).subscribe({
      next: (materials) => this.aluminumMaterials.set(materials),
      error: () => this.aluminumMaterials.set([]),
    });
  }

  reloadAccessories(): void {
    const workshopId = this.workshopContext.selectedWorkshopId();
    if (!workshopId) return;
    this.accessoriesApi.listForWorkshop(workshopId, true).subscribe({
      next: (items) => this.accessories.set(items),
      error: () => this.accessories.set([]),
    });
  }

  reloadTypes(): void {
    this.loading.set(true);
    this.error.set(null);
    this.templatesApi.listTypes(false).subscribe({
      next: (types) => {
        this.types.set(types);
        this.loading.set(false);
        if (!this.selectedTypeId() && types.length) {
          this.selectType(types[0].id);
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(apiErrorMessage(err, 'common.loadError'));
      },
    });
  }

  selectType(typeId: string): void {
    this.selectedTypeId.set(typeId);
    const type = this.types().find((t) => t.id === typeId);
    const firstVariant = type?.variants[0];
    if (firstVariant) {
      this.selectVariant(firstVariant.id);
    } else {
      this.selectedVariantId.set(null);
      this.variant.set(null);
    }
  }

  selectVariant(variantId: string): void {
    this.selectedVariantId.set(variantId);
    this.message.set(null);
    this.error.set(null);
    this.templatesApi.getVariant(variantId).subscribe({
      next: (v) => {
        this.variant.set(v);
        this.variantName = v.name;
        this.variantSashCount = v.sashCount;
        this.variantOverlapCm =
          v.overlapMm != null ? mmToCm(v.overlapMm) : null;
        this.variantIsActive = v.isActive;
      },
      error: (err) => this.error.set(apiErrorMessage(err, 'common.loadError')),
    });
  }

  saveVariantHeader(): void {
    const id = this.selectedVariantId();
    if (!id) return;
    this.saving.set(true);
    this.error.set(null);
    const overlapMm =
      this.variantOverlapCm != null
        ? cmToMm(Number(this.variantOverlapCm))
        : null;
    this.templatesApi
      .updateVariant(id, {
        name: this.variantName.trim(),
        sashCount: this.variantSashCount,
        overlapMm,
        isActive: this.variantIsActive,
      })
      .subscribe({
        next: (v) => {
          this.saving.set(false);
          this.variant.set(v);
          this.variantOverlapCm =
            v.overlapMm != null ? mmToCm(v.overlapMm) : null;
          this.message.set('templates.saved');
          this.reloadTypesQuiet();
        },
        error: (err) => {
          this.saving.set(false);
          this.error.set(apiErrorMessage(err, 'common.saveError'));
        },
      });
  }

  partSimple(part: TemplatePart): SimpleFormula | null {
    return toSimpleFormula(part.widthCoef, part.heightCoef, part.offsetMm);
  }

  glassSimpleW(slot: TemplateGlassSlot): SimpleFormula | null {
    return toSimpleFormula(slot.widthCoefW, slot.heightCoefW, slot.offsetWMm);
  }

  glassSimpleH(slot: TemplateGlassSlot): SimpleFormula | null {
    return toSimpleFormula(slot.widthCoefH, slot.heightCoefH, slot.offsetHMm);
  }

  onPartSentenceChange(
    part: TemplatePart,
    field: 'axis' | 'op' | 'nCm' | 'qty',
    value: string | number,
  ): void {
    const simple =
      this.partSimple(part) ??
      ({ axis: 'width', op: 'minus', nCm: 0 } satisfies SimpleFormula);
    if (field === 'axis') simple.axis = value as FormulaAxis;
    else if (field === 'op') simple.op = value as FormulaOp;
    else if (field === 'nCm') simple.nCm = Number(value) || 0;
    else if (field === 'qty') {
      part.qtyPerUnit = Math.max(1, Number(value) || 1);
      this.bumpVariant();
      return;
    }
    const applied = applySimpleFormula(simple);
    part.widthCoef = applied.widthCoef;
    part.heightCoef = applied.heightCoef;
    part.offsetMm = applied.offsetMm;
    this.bumpVariant();
  }

  onGlassSentenceChange(
    slot: TemplateGlassSlot,
    dim: 'W' | 'H',
    field: 'axis' | 'op' | 'nCm',
    value: string | number,
  ): void {
    const current =
      (dim === 'W' ? this.glassSimpleW(slot) : this.glassSimpleH(slot)) ??
      ({ axis: 'width', op: 'minus', nCm: 0 } satisfies SimpleFormula);
    if (field === 'axis') current.axis = value as FormulaAxis;
    else if (field === 'op') current.op = value as FormulaOp;
    else current.nCm = Number(value) || 0;
    const applied = applySimpleFormula(current);
    if (dim === 'W') {
      slot.widthCoefW = applied.widthCoef;
      slot.heightCoefW = applied.heightCoef;
      slot.offsetWMm = applied.offsetMm;
    } else {
      slot.widthCoefH = applied.widthCoef;
      slot.heightCoefH = applied.heightCoef;
      slot.offsetHMm = applied.offsetMm;
    }
    this.bumpVariant();
  }

  private bumpVariant(): void {
    const v = this.variant();
    if (!v) return;
    this.variant.set({
      ...v,
      parts: [...v.parts],
      glassSlots: [...v.glassSlots],
      accessorySlots: [...v.accessorySlots],
    });
  }

  offsetCm(mm: number): number {
    return mmToCm(Number(mm) || 0);
  }

  setOffsetCm(part: TemplatePart, cm: number): void {
    part.offsetMm = cmToMm(Number(cm) || 0);
  }

  setGlassOffsetCm(slot: TemplateGlassSlot, dim: 'W' | 'H', cm: number): void {
    const mm = cmToMm(Number(cm) || 0);
    if (dim === 'W') slot.offsetWMm = mm;
    else slot.offsetHMm = mm;
  }

  savePart(part: TemplatePart): void {
    this.saving.set(true);
    this.error.set(null);
    this.templatesApi
      .updatePart(part.id, {
        displayName: part.displayName,
        qtyPerUnit: Number(part.qtyPerUnit),
        widthCoef: Number(part.widthCoef),
        heightCoef: Number(part.heightCoef),
        offsetMm: Number(part.offsetMm),
        useSashWidthDivision: part.useSashWidthDivision,
        defaultAluminumMaterialId: part.defaultAluminumMaterialId || null,
      })
      .subscribe({
        next: (updated) => {
          this.saving.set(false);
          this.patchPart(updated);
          this.message.set('templates.saved');
        },
        error: (err) => {
          this.saving.set(false);
          this.error.set(apiErrorMessage(err, 'common.saveError'));
        },
      });
  }

  saveGlassSlot(slot: TemplateGlassSlot): void {
    this.saving.set(true);
    this.error.set(null);
    this.templatesApi
      .updateGlassSlot(slot.id, {
        name: slot.name,
        paneCount: Number(slot.paneCount),
        widthCoefW: Number(slot.widthCoefW),
        heightCoefW: Number(slot.heightCoefW),
        offsetWMm: Number(slot.offsetWMm),
        widthCoefH: Number(slot.widthCoefH),
        heightCoefH: Number(slot.heightCoefH),
        offsetHMm: Number(slot.offsetHMm),
        required: slot.required,
        useSashWidthDivision: slot.useSashWidthDivision,
      })
      .subscribe({
        next: (updated) => {
          this.saving.set(false);
          this.patchGlassSlot(updated);
          this.message.set('templates.saved');
        },
        error: (err) => {
          this.saving.set(false);
          this.error.set(apiErrorMessage(err, 'common.saveError'));
        },
      });
  }

  saveAccessorySlot(slot: TemplateAccessorySlot): void {
    this.saving.set(true);
    this.error.set(null);
    this.templatesApi
      .updateAccessorySlot(slot.id, {
        name: slot.name,
        quantityRule: slot.quantityRule,
        qty: Number(slot.qty),
        defaultAccessoryId: slot.defaultAccessoryId || null,
      })
      .subscribe({
        next: (updated) => {
          this.saving.set(false);
          this.patchAccessorySlot(updated);
          this.message.set('templates.saved');
        },
        error: (err) => {
          this.saving.set(false);
          this.error.set(apiErrorMessage(err, 'common.saveError'));
        },
      });
  }

  openQuickAddAluminum(partId: string): void {
    this.quickAddTargetPartId.set(partId);
    this.quickAddTargetSlotId.set(null);
    this.quickAddMode.set('aluminum');
  }

  openQuickAddAccessory(slotId: string): void {
    this.quickAddTargetSlotId.set(slotId);
    this.quickAddTargetPartId.set(null);
    this.quickAddMode.set('accessory');
  }

  onQuickAddClosed(): void {
    this.quickAddMode.set(null);
    this.quickAddTargetPartId.set(null);
    this.quickAddTargetSlotId.set(null);
  }

  onQuickAddCreated(result: QuickAddResult): void {
    if (result.mode === 'aluminum') {
      this.reloadAluminum();
      const partId = this.quickAddTargetPartId();
      const v = this.variant();
      if (partId && v) {
        const part = v.parts.find((p) => p.id === partId);
        if (part) {
          part.defaultAluminumMaterialId = result.id;
          this.patchPart({ ...part });
        }
      }
    } else if (result.mode === 'accessory') {
      this.reloadAccessories();
      const slotId = this.quickAddTargetSlotId();
      const v = this.variant();
      if (slotId && v) {
        const slot = v.accessorySlots.find((s) => s.id === slotId);
        if (slot) {
          slot.defaultAccessoryId = result.id;
          this.patchAccessorySlot({ ...slot });
        }
      }
    }
    this.onQuickAddClosed();
  }

  onMaterialSelect(part: TemplatePart, value: string): void {
    if (value === '__add__') {
      this.openQuickAddAluminum(part.id);
      return;
    }
    part.defaultAluminumMaterialId = value || null;
  }

  onAccessorySelect(slot: TemplateAccessorySlot, value: string): void {
    if (value === '__add__') {
      this.openQuickAddAccessory(slot.id);
      return;
    }
    slot.defaultAccessoryId = value || null;
  }

  private reloadTypesQuiet(): void {
    this.templatesApi.listTypes(false).subscribe({
      next: (types) => this.types.set(types),
    });
  }

  private patchPart(updated: TemplatePart): void {
    const v = this.variant();
    if (!v) return;
    this.variant.set({
      ...v,
      parts: v.parts.map((p) => (p.id === updated.id ? updated : p)),
    });
  }

  private patchGlassSlot(updated: TemplateGlassSlot): void {
    const v = this.variant();
    if (!v) return;
    this.variant.set({
      ...v,
      glassSlots: v.glassSlots.map((s) => (s.id === updated.id ? updated : s)),
    });
  }

  private patchAccessorySlot(updated: TemplateAccessorySlot): void {
    const v = this.variant();
    if (!v) return;
    this.variant.set({
      ...v,
      accessorySlots: v.accessorySlots.map((s) =>
        s.id === updated.id ? updated : s,
      ),
    });
  }
}
