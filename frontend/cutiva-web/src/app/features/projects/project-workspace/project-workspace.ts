import { DecimalPipe } from '@angular/common';
import { Component, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { forkJoin, of, switchMap } from 'rxjs';
import { AccessoriesApi, Accessory } from '../../../core/api/accessories.api';
import { GlassApi, GlassType } from '../../../core/api/glass.api';
import { Material, MaterialsApi } from '../../../core/api/materials.api';
import {
  ProductTemplatesApi,
  ProductType,
  ProductVariant,
  ProductVariantSummary,
} from '../../../core/api/product-templates.api';
import {
  CalculateProjectResult,
  CreateProjectItemBody,
  InspectionStatus,
  ProjectItem,
  ProjectItemSummary,
  ProjectItemsApi,
} from '../../../core/api/project-items.api';
import { ProjectsApi } from '../../../core/api/projects.api';
import {
  ProjectSection,
  ProjectSectionsApi,
} from '../../../core/api/project-sections.api';
import {
  RequiredCut,
  RequiredCutsApi,
} from '../../../core/api/required-cuts.api';
import { Sector, SectorsApi } from '../../../core/api/sectors.api';
import { MARKET_MATERIALS_SECTOR_CODE } from '../../../core/constants/market';
import { apiErrorMessage, isI18nKey } from '../../../core/http/api-error';
import { AuthService } from '../../../core/services/auth.service';
import { PlanEntitlementsService } from '../../../core/services/plan-entitlements.service';
import { WorkshopContextService } from '../../../core/services/workshop-context.service';
import { cmToMm, formatCm, mmToCm } from '../../../core/units/length';
import { FramePreview } from '../../../shared/frame-preview/frame-preview';
import {
  QuickAddMode,
  QuickAddResult,
  ResourceQuickAdd,
} from '../../../shared/resource-quick-add/resource-quick-add';

export interface ElementRow {
  section: ProjectSection;
  cuts: RequiredCut[];
  materialCode: string;
}

interface AccessoryDraftLine {
  accessoryId: string;
  quantity: number;
}

interface PartChoiceDraft {
  templatePartId: string;
  role: string;
  displayName: string;
  materialId: string;
}

interface GlassChoiceDraft {
  templateGlassSlotId: string;
  slotName: string;
  required: boolean;
  glassSpecId: string;
}

interface AccessoryChoiceDraft {
  templateAccessorySlotId: string;
  slotName: string;
  accessoryId: string;
  quantityOverride: number | null;
}

@Component({
  selector: 'app-project-workspace',
  imports: [
    FormsModule,
    RouterLink,
    TranslatePipe,
    FramePreview,
    DecimalPipe,
    ResourceQuickAdd,
  ],
  templateUrl: './project-workspace.html',
  styleUrl: './project-workspace.scss',
})
export class ProjectWorkspace {
  private readonly route = inject(ActivatedRoute);
  private readonly sectionsApi = inject(ProjectSectionsApi);
  private readonly cutsApi = inject(RequiredCutsApi);
  private readonly sectorsApi = inject(SectorsApi);
  private readonly projectsApi = inject(ProjectsApi);
  private readonly accessoriesApi = inject(AccessoriesApi);
  private readonly materialsApi = inject(MaterialsApi);
  private readonly glassApi = inject(GlassApi);
  private readonly templatesApi = inject(ProductTemplatesApi);
  private readonly projectItemsApi = inject(ProjectItemsApi);
  private readonly workshopContext = inject(WorkshopContextService);
  private readonly translate = inject(TranslateService);
  private readonly auth = inject(AuthService);
  readonly entitlements = inject(PlanEntitlementsService);
  readonly canManageCatalog = this.auth.canManageCatalog;

  readonly projectId = signal<string>('');
  readonly projectName = signal<string>('');
  readonly sections = signal<ProjectSection[]>([]);
  readonly rows = signal<ElementRow[]>([]);
  readonly elementTypes = signal<Sector[]>([]);
  readonly catalog = signal<Accessory[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);

  readonly productTypes = signal<ProductType[]>([]);
  readonly aluminumMaterials = signal<Material[]>([]);
  readonly glassTypes = signal<GlassType[]>([]);
  readonly productItems = signal<ProjectItemSummary[]>([]);
  readonly selectedVariant = signal<ProductVariant | null>(null);
  readonly editingItemId = signal<string | null>(null);
  readonly calculateResult = signal<CalculateProjectResult | null>(null);
  readonly calculating = signal(false);
  readonly itemSaving = signal(false);

  readonly isI18nKey = isI18nKey;
  readonly formatCm = formatCm;

  elementTypeId = '';
  elementName = '';
  heightCm = 120;
  widthCm = 100;
  quantity = 1;
  includeAccessories = false;
  accessoryLines: AccessoryDraftLine[] = [];

  // Product item wizard
  itemTypeId = '';
  itemVariantId = '';
  itemLabel = '';
  itemWidthCm = 100;
  itemHeightCm = 120;
  itemQty = 1;
  partChoices: PartChoiceDraft[] = [];
  glassChoices: GlassChoiceDraft[] = [];
  accessoryChoices: AccessoryChoiceDraft[] = [];
  inspectionLocation = '';
  /** Site survey measured sizes in cm (stored as mm). */
  inspectionMeasuredWCm: number | null = null;
  inspectionMeasuredHCm: number | null = null;
  inspectionNotes = '';
  inspectionStatus: InspectionStatus = 'Pending';
  inspectionBy = '';
  /** Kerf in cm (API expects mm). */
  kerfCm = 0.3;
  useRemnants = true;

  readonly quickAddMode = signal<QuickAddMode | null>(null);
  readonly quickAddPartIndex = signal<number | null>(null);
  readonly quickAddGlassIndex = signal<number | null>(null);
  readonly quickAddAccessoryIndex = signal<number | null>(null);
  readonly quickAddLegacyAccessoryIndex = signal<number | null>(null);

  constructor() {
    this.route.paramMap.subscribe((params) => {
      const id = params.get('projectId') ?? '';
      this.projectId.set(id);
      if (id) this.bootstrap(id);
    });

    effect(() => {
      const workshopId = this.workshopContext.selectedWorkshopId();
      const loaded = this.workshopContext.loaded();
      if (!loaded || !workshopId) {
        this.catalog.set([]);
        this.glassTypes.set([]);
        return;
      }
      this.accessoriesApi.listForWorkshop(workshopId).subscribe({
        next: (items) => this.catalog.set(items),
        error: () => this.catalog.set([]),
      });
      if (this.entitlements.productTemplates()) {
        this.glassApi.listTypes(workshopId, true).subscribe({
          next: (types) => this.glassTypes.set(types),
          error: () => this.glassTypes.set([]),
        });
      }
    });
  }

  get flatGlassSpecs(): { id: string; label: string }[] {
    return this.glassTypes().flatMap((t) =>
      t.specs
        .filter((s) => s.isActive)
        .map((s) => ({
          id: s.id,
          label: `${t.name} · ${s.thicknessMm}mm (${s.pricePerSqm})`,
        })),
    );
  }

  variantsForSelectedType(): ProductVariantSummary[] {
    return (
      this.productTypes().find((t) => t.id === this.itemTypeId)?.variants ?? []
    );
  }

  selectedType(): Sector | null {
    return (
      this.elementTypes().find((s) => s.id === this.elementTypeId) ?? null
    );
  }

  canSubmitElement(): boolean {
    const type = this.selectedType();
    if (!type || this.saving()) return false;
    return !!type.materialId;
  }

  madeFromLabel(): string {
    const type = this.selectedType();
    if (!type?.materialCode) return '';
    return type.materialName
      ? `${type.materialCode} — ${type.materialName}`
      : type.materialCode;
  }

  draftHeightMm(): number | null {
    const h = Number(this.heightCm);
    return h > 0 ? cmToMm(h) : null;
  }

  draftWidthMm(): number | null {
    const w = Number(this.widthCm);
    return w > 0 ? cmToMm(w) : null;
  }

  itemPreviewHeightMm(): number | null {
    const h = Number(this.itemHeightCm);
    return h > 0 ? cmToMm(h) : null;
  }

  itemPreviewWidthMm(): number | null {
    const w = Number(this.itemWidthCm);
    return w > 0 ? cmToMm(w) : null;
  }

  selectedProductType(): ProductType | null {
    return this.productTypes().find((t) => t.id === this.itemTypeId) ?? null;
  }

  itemPreviewKindCode(): string {
    return this.selectedProductType()?.kind ?? '';
  }

  itemPreviewKindLabel(): string {
    const type = this.selectedProductType();
    return type?.name ?? type?.kind ?? '';
  }

  sizeLabel(section: ProjectSection): string {
    if (section.heightMm == null || section.widthMm == null) return '—';
    return `${formatCm(section.heightMm)} × ${formatCm(section.widthMm)}`;
  }

  pieceSummary(row: ElementRow): string {
    if (row.cuts.length === 0) return '';
    return row.cuts
      .map((c) => `${formatCm(c.lengthMm)}×${c.quantity}`)
      .join(' · ');
  }

  accessoriesSummary(section: ProjectSection): string {
    const lines = section.accessories ?? [];
    if (lines.length === 0) return this.translate.instant('accessories.none');
    return lines.map((a) => `${a.name}×${a.quantity}`).join(' · ');
  }

  onToggleAccessories(): void {
    if (this.includeAccessories && this.accessoryLines.length === 0) {
      this.addAccessoryLine();
    }
    if (!this.includeAccessories) {
      this.accessoryLines = [];
    }
  }

  addAccessoryLine(): void {
    const first = this.catalog()[0];
    this.accessoryLines = [
      ...this.accessoryLines,
      { accessoryId: first?.id ?? '', quantity: 1 },
    ];
  }

  removeAccessoryLine(index: number): void {
    this.accessoryLines = this.accessoryLines.filter((_, i) => i !== index);
  }

  private bootstrap(projectId: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.resolveProjectName(projectId);

    this.sectorsApi.list().subscribe({
      next: (sectors) => {
        const types = sectors.filter(
          (s) =>
            s.code.toUpperCase() !== MARKET_MATERIALS_SECTOR_CODE &&
            !!s.materialId,
        );
        this.elementTypes.set(types);
        if (!this.elementTypeId && types.length) {
          this.elementTypeId = types[0].id;
        } else if (
          this.elementTypeId &&
          !types.some((t) => t.id === this.elementTypeId)
        ) {
          this.elementTypeId = types[0]?.id ?? '';
        }
      },
    });

    this.reloadRows(projectId);

    if (this.entitlements.productTemplates()) {
      this.reloadProductItems(projectId);
      this.templatesApi.listTypes(true).subscribe({
        next: (types) => {
          this.productTypes.set(types);
          if (!this.itemTypeId && types.length) {
            this.itemTypeId = types[0].id;
            const firstVariant = types[0].variants[0];
            if (firstVariant) this.onVariantPicked(firstVariant.id);
          }
        },
        error: () => this.productTypes.set([]),
      });
      this.materialsApi.list({ category: 'Aluminum', activeOnly: true }).subscribe({
        next: (materials) => this.aluminumMaterials.set(materials),
        error: () => this.aluminumMaterials.set([]),
      });
    }
  }

  reloadProductItems(projectId = this.projectId()): void {
    if (!projectId || !this.entitlements.productTemplates()) return;
    this.projectItemsApi.list(projectId).subscribe({
      next: (items) => this.productItems.set(items),
      error: (err) => this.error.set(apiErrorMessage(err, 'common.loadError')),
    });
  }

  onTypePicked(): void {
    const variants = this.variantsForSelectedType();
    const first = variants[0];
    this.itemVariantId = first?.id ?? '';
    if (first) this.onVariantPicked(first.id);
    else {
      this.selectedVariant.set(null);
      this.partChoices = [];
      this.glassChoices = [];
      this.accessoryChoices = [];
    }
  }

  onVariantPicked(variantId: string): void {
    this.itemVariantId = variantId;
    if (!variantId) {
      this.selectedVariant.set(null);
      return;
    }
    this.templatesApi.getVariant(variantId).subscribe({
      next: (variant) => {
        this.selectedVariant.set(variant);
        this.seedChoicesFromVariant(variant);
        if (!this.itemLabel.trim()) {
          this.itemLabel = variant.name;
        }
      },
      error: (err) => this.error.set(apiErrorMessage(err, 'common.loadError')),
    });
  }

  private seedChoicesFromVariant(variant: ProductVariant): void {
    const firstMaterial = this.aluminumMaterials()[0]?.id ?? '';
    this.partChoices = variant.parts.map((p) => ({
      templatePartId: p.id,
      role: p.role,
      displayName: p.displayName,
      materialId: p.defaultAluminumMaterialId ?? firstMaterial,
    }));
    this.glassChoices = variant.glassSlots.map((s) => ({
      templateGlassSlotId: s.id,
      slotName: s.name,
      required: s.required,
      glassSpecId: this.flatGlassSpecs[0]?.id ?? '',
    }));
    this.accessoryChoices = variant.accessorySlots.map((s) => ({
      templateAccessorySlotId: s.id,
      slotName: s.name,
      accessoryId: s.defaultAccessoryId ?? this.catalog()[0]?.id ?? '',
      quantityOverride: null,
    }));
  }

  resetItemWizard(): void {
    this.editingItemId.set(null);
    this.itemLabel = '';
    this.itemWidthCm = 100;
    this.itemHeightCm = 120;
    this.itemQty = 1;
    this.inspectionLocation = '';
    this.inspectionMeasuredWCm = null;
    this.inspectionMeasuredHCm = null;
    this.inspectionNotes = '';
    this.inspectionStatus = 'Pending';
    this.inspectionBy = '';
    const variant = this.selectedVariant();
    if (variant) this.seedChoicesFromVariant(variant);
  }

  private buildItemBody(): CreateProjectItemBody | null {
    const variantId = this.itemVariantId;
    const label = this.itemLabel.trim();
    const widthMm = cmToMm(Number(this.itemWidthCm));
    const heightMm = cmToMm(Number(this.itemHeightCm));
    const quantity = Number(this.itemQty);

    if (!variantId || !label || widthMm <= 0 || heightMm <= 0 || quantity < 1) {
      this.error.set('projectItems.validation.required');
      return null;
    }

    for (const part of this.partChoices) {
      if (!part.materialId) {
        this.error.set('projectItems.validation.materials');
        return null;
      }
    }
    for (const glass of this.glassChoices) {
      if (glass.required && !glass.glassSpecId) {
        this.error.set('projectItems.validation.glass');
        return null;
      }
    }

    return {
      productVariantId: variantId,
      label,
      widthMm,
      heightMm,
      quantity,
      partChoices: this.partChoices.map((p) => ({
        templatePartId: p.templatePartId,
        materialId: p.materialId,
      })),
      glassChoices: this.glassChoices
        .filter((g) => g.glassSpecId)
        .map((g) => ({
          templateGlassSlotId: g.templateGlassSlotId,
          glassSpecId: g.glassSpecId,
        })),
      accessoryChoices: this.accessoryChoices
        .filter((a) => a.accessoryId)
        .map((a) => ({
          templateAccessorySlotId: a.templateAccessorySlotId,
          accessoryId: a.accessoryId,
          quantityOverride: a.quantityOverride,
        })),
      inspection: {
        locationLabel: this.inspectionLocation.trim() || null,
        measuredWidthMm:
          this.inspectionMeasuredWCm != null
            ? cmToMm(Number(this.inspectionMeasuredWCm))
            : null,
        measuredHeightMm:
          this.inspectionMeasuredHCm != null
            ? cmToMm(Number(this.inspectionMeasuredHCm))
            : null,
        inspectedBy: this.inspectionBy.trim() || null,
        notes: this.inspectionNotes.trim() || null,
        status: this.inspectionStatus,
      },
    };
  }

  saveProductItem(): void {
    const body = this.buildItemBody();
    if (!body) return;

    const projectId = this.projectId();
    const editingId = this.editingItemId();
    this.itemSaving.set(true);
    this.error.set(null);

    if (editingId) {
      this.projectItemsApi
        .update(editingId, {
          label: body.label,
          widthMm: body.widthMm,
          heightMm: body.heightMm,
          quantity: body.quantity,
          partChoices: body.partChoices,
          glassChoices: body.glassChoices,
          accessoryChoices: body.accessoryChoices,
        })
        .pipe(
          switchMap((item) => {
            if (!body.inspection) return of(item);
            return this.projectItemsApi
              .upsertInspection(item.id, body.inspection)
              .pipe(switchMap(() => of(item)));
          }),
        )
        .subscribe({
          next: () => {
            this.itemSaving.set(false);
            this.resetItemWizard();
            this.reloadProductItems(projectId);
          },
          error: (err) => {
            this.itemSaving.set(false);
            this.error.set(apiErrorMessage(err, 'common.saveError'));
          },
        });
      return;
    }

    this.projectItemsApi.create(projectId, body).subscribe({
      next: () => {
        this.itemSaving.set(false);
        this.resetItemWizard();
        this.reloadProductItems(projectId);
      },
      error: (err) => {
        this.itemSaving.set(false);
        this.error.set(apiErrorMessage(err, 'common.saveError'));
      },
    });
  }

  editProductItem(summary: ProjectItemSummary): void {
    this.projectItemsApi.getById(summary.id).subscribe({
      next: (item) => this.loadItemIntoWizard(item),
      error: (err) => this.error.set(apiErrorMessage(err, 'common.loadError')),
    });
  }

  private loadItemIntoWizard(item: ProjectItem): void {
    this.editingItemId.set(item.id);
    this.itemLabel = item.label;
    this.itemWidthCm = item.widthMm / 10;
    this.itemHeightCm = item.heightMm / 10;
    this.itemQty = item.quantity;
    this.itemVariantId = item.productVariantId;

    const type = this.productTypes().find((t) =>
      t.variants.some((v) => v.id === item.productVariantId),
    );
    this.itemTypeId = type?.id ?? '';

    this.templatesApi.getVariant(item.productVariantId).subscribe({
      next: (variant) => {
        this.selectedVariant.set(variant);
        this.partChoices = variant.parts.map((p) => {
          const choice = item.partChoices.find((c) => c.templatePartId === p.id);
          return {
            templatePartId: p.id,
            role: p.role,
            displayName: p.displayName,
            materialId: choice?.materialId ?? p.defaultAluminumMaterialId ?? '',
          };
        });
        this.glassChoices = variant.glassSlots.map((s) => {
          const choice = item.glassChoices.find(
            (c) => c.templateGlassSlotId === s.id,
          );
          return {
            templateGlassSlotId: s.id,
            slotName: s.name,
            required: s.required,
            glassSpecId: choice?.glassSpecId ?? '',
          };
        });
        this.accessoryChoices = variant.accessorySlots.map((s) => {
          const choice = item.accessoryChoices.find(
            (c) => c.templateAccessorySlotId === s.id,
          );
          return {
            templateAccessorySlotId: s.id,
            slotName: s.name,
            accessoryId: choice?.accessoryId ?? s.defaultAccessoryId ?? '',
            quantityOverride: choice?.quantityOverride ?? null,
          };
        });
      },
    });

    if (item.inspection) {
      this.inspectionLocation = item.inspection.locationLabel ?? '';
      this.inspectionMeasuredWCm =
        item.inspection.measuredWidthMm != null
          ? mmToCm(item.inspection.measuredWidthMm)
          : null;
      this.inspectionMeasuredHCm =
        item.inspection.measuredHeightMm != null
          ? mmToCm(item.inspection.measuredHeightMm)
          : null;
      this.inspectionNotes = item.inspection.notes ?? '';
      this.inspectionStatus = item.inspection.status;
      this.inspectionBy = item.inspection.inspectedBy ?? '';
    }
  }

  deleteProductItem(item: ProjectItemSummary): void {
    if (!confirm(this.translate.instant('projectItems.deleteConfirm'))) return;
    this.projectItemsApi.delete(item.id).subscribe({
      next: () => this.reloadProductItems(),
      error: (err) => this.error.set(apiErrorMessage(err, 'common.deleteError')),
    });
  }

  duplicateProductItem(item: ProjectItemSummary): void {
    this.projectItemsApi.duplicate(item.id).subscribe({
      next: () => this.reloadProductItems(),
      error: (err) => this.error.set(apiErrorMessage(err, 'common.saveError')),
    });
  }

  runCalculate(): void {
    const projectId = this.projectId();
    if (!projectId) return;
    this.calculating.set(true);
    this.error.set(null);
    this.projectItemsApi
      .calculate(projectId, {
        kerfMm: cmToMm(Number(this.kerfCm) || 0),
        useRemnants: this.useRemnants,
      })
      .subscribe({
        next: (result) => {
          this.calculating.set(false);
          this.calculateResult.set(result);
          this.reloadProductItems(projectId);
        },
        error: (err) => {
          this.calculating.set(false);
          this.error.set(apiErrorMessage(err, 'common.saveError'));
        },
      });
  }

  itemSizeLabel(item: ProjectItemSummary): string {
    return `${formatCm(item.widthMm)} × ${formatCm(item.heightMm)}`;
  }

  private resolveProjectName(projectId: string): void {
    const workshopId = this.workshopContext.selectedWorkshopId();
    if (!workshopId) {
      this.projectName.set('');
      return;
    }

    this.projectsApi.list(workshopId, 1, 100).subscribe({
      next: (page) => {
        const match = page.items.find((p) => p.id === projectId);
        this.projectName.set(match?.name ?? '');
      },
      error: () => this.projectName.set(''),
    });
  }

  private reloadRows(projectId = this.projectId()): void {
    this.sectionsApi.list(projectId).subscribe({
      next: (sections) => {
        this.sections.set(sections);
        if (sections.length === 0) {
          this.rows.set([]);
          this.loading.set(false);
          return;
        }

        forkJoin(
          sections.map((section) =>
            this.cutsApi.list({ projectSectionId: section.id }),
          ),
        ).subscribe({
          next: (groups) => {
            const rows: ElementRow[] = sections.map((section, i) => {
              const cuts = groups[i] ?? [];
              return {
                section: {
                  ...section,
                  accessories: section.accessories ?? [],
                },
                cuts,
                materialCode: cuts[0]?.materialCode ?? '—',
              };
            });
            this.rows.set(rows);
            this.loading.set(false);
          },
          error: (err) => {
            this.error.set(apiErrorMessage(err, 'common.loadError'));
            this.loading.set(false);
          },
        });
      },
      error: (err) => {
        this.error.set(apiErrorMessage(err, 'common.loadError'));
        this.loading.set(false);
      },
    });
  }

  addElement(): void {
    this.error.set(null);

    const type = this.selectedType();
    if (!type) {
      this.error.set('projects.validation.typeRequired');
      return;
    }

    if (!type.materialId) {
      this.error.set('projects.validation.typeNeedsMaterial');
      return;
    }

    const height = Number(this.heightCm);
    const width = Number(this.widthCm);
    const qty = Number(this.quantity);
    if (
      !height ||
      height <= 0 ||
      !width ||
      width <= 0 ||
      !qty ||
      qty < 1
    ) {
      this.error.set('projects.validation.measureRequired');
      return;
    }

    const accessories = this.includeAccessories
      ? this.accessoryLines
          .filter((l) => l.accessoryId && l.quantity > 0)
          .map((l) => ({
            accessoryId: l.accessoryId,
            quantity: Number(l.quantity),
          }))
      : [];

    const typeName = type.name;
    const countForType =
      this.sections().filter((s) => s.sectorId === this.elementTypeId).length +
      1;
    const name = this.elementName.trim() || `${typeName} ${countForType}`;
    const heightMm = cmToMm(height);
    const widthMm = cmToMm(width);
    const pieceQty = 2 * qty;
    const heightSuffix = this.translate.instant('projects.cutHeightSuffix');
    const widthSuffix = this.translate.instant('projects.cutWidthSuffix');

    this.saving.set(true);

    this.sectionsApi
      .create(this.projectId(), {
        name,
        sectorId: this.elementTypeId,
        heightMm,
        widthMm,
        unitQuantity: qty,
        accessories,
      })
      .pipe(
        switchMap((section) =>
          this.cutsApi
            .create({
              projectSectionId: section.id,
              materialId: type.materialId!,
              name: `${name} — ${heightSuffix}`,
              lengthMm: heightMm,
              quantity: pieceQty,
            })
            .pipe(
              switchMap(() =>
                this.cutsApi.create({
                  projectSectionId: section.id,
                  materialId: type.materialId!,
                  name: `${name} — ${widthSuffix}`,
                  lengthMm: widthMm,
                  quantity: pieceQty,
                }),
              ),
              switchMap(() => of(section.id)),
            ),
        ),
      )
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.elementName = '';
          this.heightCm = 120;
          this.widthCm = 100;
          this.quantity = 1;
          this.includeAccessories = false;
          this.accessoryLines = [];
          this.reloadRows();
        },
        error: (err) => {
          this.saving.set(false);
          this.error.set(apiErrorMessage(err, 'common.saveError'));
        },
      });
  }

  removeRow(row: ElementRow): void {
    this.sectionsApi.delete(this.projectId(), row.section.id).subscribe({
      next: () => this.reloadRows(),
      error: (err) => this.error.set(apiErrorMessage(err, 'common.deleteError')),
    });
  }

  onPartMaterialSelect(index: number, value: string): void {
    if (value === '__add__') {
      this.quickAddPartIndex.set(index);
      this.quickAddGlassIndex.set(null);
      this.quickAddAccessoryIndex.set(null);
      this.quickAddLegacyAccessoryIndex.set(null);
      this.quickAddMode.set('aluminum');
      return;
    }
    if (this.partChoices[index]) {
      this.partChoices[index].materialId = value;
    }
  }

  onGlassSpecSelect(index: number, value: string): void {
    if (value === '__add__') {
      this.quickAddGlassIndex.set(index);
      this.quickAddPartIndex.set(null);
      this.quickAddAccessoryIndex.set(null);
      this.quickAddLegacyAccessoryIndex.set(null);
      this.quickAddMode.set('glassSpec');
      return;
    }
    if (this.glassChoices[index]) {
      this.glassChoices[index].glassSpecId = value;
    }
  }

  onAccessoryChoiceSelect(index: number, value: string): void {
    if (value === '__add__') {
      this.quickAddAccessoryIndex.set(index);
      this.quickAddPartIndex.set(null);
      this.quickAddGlassIndex.set(null);
      this.quickAddLegacyAccessoryIndex.set(null);
      this.quickAddMode.set('accessory');
      return;
    }
    if (this.accessoryChoices[index]) {
      this.accessoryChoices[index].accessoryId = value;
    }
  }

  onLegacyAccessorySelect(index: number, value: string): void {
    if (value === '__add__') {
      this.quickAddLegacyAccessoryIndex.set(index);
      this.quickAddPartIndex.set(null);
      this.quickAddGlassIndex.set(null);
      this.quickAddAccessoryIndex.set(null);
      this.quickAddMode.set('accessory');
      return;
    }
    if (this.accessoryLines[index]) {
      this.accessoryLines[index].accessoryId = value;
    }
  }

  onQuickAddClosed(): void {
    this.quickAddMode.set(null);
    this.quickAddPartIndex.set(null);
    this.quickAddGlassIndex.set(null);
    this.quickAddAccessoryIndex.set(null);
    this.quickAddLegacyAccessoryIndex.set(null);
  }

  onQuickAddCreated(result: QuickAddResult): void {
    if (result.mode === 'aluminum') {
      this.materialsApi
        .list({ category: 'Aluminum', activeOnly: true })
        .subscribe({
          next: (materials) => {
            this.aluminumMaterials.set(materials);
            const idx = this.quickAddPartIndex();
            if (idx != null && this.partChoices[idx]) {
              this.partChoices[idx].materialId = result.id;
            }
            this.onQuickAddClosed();
          },
          error: () => this.onQuickAddClosed(),
        });
      return;
    }

    if (result.mode === 'glassSpec') {
      const workshopId = this.workshopContext.selectedWorkshopId();
      if (!workshopId) {
        this.onQuickAddClosed();
        return;
      }
      this.glassApi.listTypes(workshopId, true).subscribe({
        next: (types) => {
          this.glassTypes.set(types);
          const idx = this.quickAddGlassIndex();
          if (idx != null && this.glassChoices[idx]) {
            this.glassChoices[idx].glassSpecId = result.id;
          }
          this.onQuickAddClosed();
        },
        error: () => this.onQuickAddClosed(),
      });
      return;
    }

    const workshopId = this.workshopContext.selectedWorkshopId();
    if (!workshopId) {
      this.onQuickAddClosed();
      return;
    }
    this.accessoriesApi.listForWorkshop(workshopId).subscribe({
      next: (items) => {
        this.catalog.set(items);
        const accIdx = this.quickAddAccessoryIndex();
        if (accIdx != null && this.accessoryChoices[accIdx]) {
          this.accessoryChoices[accIdx].accessoryId = result.id;
        }
        const legIdx = this.quickAddLegacyAccessoryIndex();
        if (legIdx != null && this.accessoryLines[legIdx]) {
          this.accessoryLines[legIdx].accessoryId = result.id;
        }
        this.onQuickAddClosed();
      },
      error: () => this.onQuickAddClosed(),
    });
  }
}
