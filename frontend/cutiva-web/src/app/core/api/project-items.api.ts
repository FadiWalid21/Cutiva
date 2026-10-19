import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import type { TemplatePartRole } from './product-templates.api';

export type InspectionStatus = 'Pending' | 'Done';

export interface ProjectItemPartChoiceInput {
  templatePartId: string;
  materialId: string;
}

export interface ProjectItemGlassChoiceInput {
  templateGlassSlotId: string;
  glassSpecId: string;
}

export interface ProjectItemAccessoryChoiceInput {
  templateAccessorySlotId: string;
  accessoryId: string;
  quantityOverride?: number | null;
}

export interface ProjectItemInspectionInput {
  locationLabel?: string | null;
  measuredWidthMm?: number | null;
  measuredHeightMm?: number | null;
  inspectedAtUtc?: string | null;
  inspectedBy?: string | null;
  notes?: string | null;
  status?: InspectionStatus;
}

export interface ProjectItemPartChoice {
  id: string;
  templatePartId: string;
  role: TemplatePartRole;
  materialId: string;
  materialCode: string;
  materialName: string;
}

export interface ProjectItemGlassChoice {
  id: string;
  templateGlassSlotId: string;
  slotName: string;
  glassSpecId: string;
  thicknessMm: number;
  pricePerSqm: number;
}

export interface ProjectItemAccessoryChoice {
  id: string;
  templateAccessorySlotId: string;
  slotName: string;
  accessoryId: string;
  accessoryName: string;
  unitPrice: number;
  quantityOverride: number | null;
}

export interface ProjectItemInspection {
  id: string;
  locationLabel: string | null;
  measuredWidthMm: number | null;
  measuredHeightMm: number | null;
  inspectedAtUtc: string | null;
  inspectedBy: string | null;
  notes: string | null;
  status: InspectionStatus;
}

export interface ProjectItem {
  id: string;
  projectId: string;
  productVariantId: string;
  productVariantName: string;
  productVariantCode: string;
  label: string;
  widthMm: number;
  heightMm: number;
  quantity: number;
  effectiveWidthMm: number;
  effectiveHeightMm: number;
  calculationSnapshotJson: string | null;
  lastCalculatedAtUtc: string | null;
  inspection: ProjectItemInspection | null;
  partChoices: ProjectItemPartChoice[];
  glassChoices: ProjectItemGlassChoice[];
  accessoryChoices: ProjectItemAccessoryChoice[];
}

export interface ProjectItemSummary {
  id: string;
  productVariantId: string;
  productVariantName: string;
  label: string;
  widthMm: number;
  heightMm: number;
  quantity: number;
  effectiveWidthMm: number;
  effectiveHeightMm: number;
  lastCalculatedAtUtc: string | null;
}

export interface CreateProjectItemBody {
  productVariantId: string;
  label: string;
  widthMm: number;
  heightMm: number;
  quantity: number;
  partChoices: ProjectItemPartChoiceInput[];
  glassChoices: ProjectItemGlassChoiceInput[];
  accessoryChoices: ProjectItemAccessoryChoiceInput[];
  inspection?: ProjectItemInspectionInput | null;
}

export interface UpdateProjectItemBody {
  label: string;
  widthMm: number;
  heightMm: number;
  quantity: number;
  partChoices: ProjectItemPartChoiceInput[];
  glassChoices: ProjectItemGlassChoiceInput[];
  accessoryChoices: ProjectItemAccessoryChoiceInput[];
}

export interface CalculateProjectBody {
  kerfMm?: number;
  useRemnants?: boolean;
}

export interface PurchaseBarLine {
  materialId: string;
  materialCode: string;
  materialName: string;
  lengthMm: number;
  quantity: number;
  unitPrice: number;
  lineCost: number;
  isRemnant: boolean;
}

export interface CalculateGlassPane {
  projectItemId: string;
  itemLabel: string;
  glassSpecId: string;
  name: string;
  widthMm: number;
  heightMm: number;
  areaM2PerPane: number;
  paneQuantity: number;
  pricePerSqm: number;
  lineCost: number;
}

export interface CalculateAccessoryLine {
  projectItemId: string;
  itemLabel: string;
  accessoryId: string;
  name: string;
  quantity: number;
  unitPrice: number;
  lineCost: number;
}

export interface CalculateItemCost {
  projectItemId: string;
  label: string;
  effectiveWidthMm: number;
  effectiveHeightMm: number;
  aluminumCost: number;
  glassCost: number;
  accessoryCost: number;
  totalCost: number;
}

export interface CalculateUnallocated {
  materialId: string;
  materialCode: string;
  name: string;
  lengthMm: number;
  quantity: number;
}

export interface CalculateMaterialWaste {
  materialId: string;
  materialCode: string;
  totalKerfMm: number;
  totalLeftoverMm: number;
  wastePercent: number;
  isComplete: boolean;
}

export interface CalculateProjectResult {
  projectId: string;
  totalAluminumCost: number;
  totalGlassCost: number;
  totalAccessoryCost: number;
  grandTotal: number;
  purchaseBars: PurchaseBarLine[];
  glassPanes: CalculateGlassPane[];
  accessories: CalculateAccessoryLine[];
  itemCosts: CalculateItemCost[];
  unallocated: CalculateUnallocated[];
  wasteByMaterial: CalculateMaterialWaste[];
}

@Injectable({ providedIn: 'root' })
export class ProjectItemsApi {
  private readonly http = inject(HttpClient);

  list(projectId: string): Observable<ProjectItemSummary[]> {
    return this.http.get<ProjectItemSummary[]>(
      `${API_BASE_URL}/api/projects/${projectId}/items`,
    );
  }

  getById(id: string): Observable<ProjectItem> {
    return this.http.get<ProjectItem>(
      `${API_BASE_URL}/api/project-items/${id}`,
    );
  }

  create(
    projectId: string,
    body: CreateProjectItemBody,
  ): Observable<ProjectItem> {
    return this.http.post<ProjectItem>(
      `${API_BASE_URL}/api/projects/${projectId}/items`,
      body,
    );
  }

  update(id: string, body: UpdateProjectItemBody): Observable<ProjectItem> {
    return this.http.put<ProjectItem>(
      `${API_BASE_URL}/api/project-items/${id}`,
      body,
    );
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${API_BASE_URL}/api/project-items/${id}`);
  }

  duplicate(id: string): Observable<ProjectItem> {
    return this.http.post<ProjectItem>(
      `${API_BASE_URL}/api/project-items/${id}/duplicate`,
      {},
    );
  }

  upsertInspection(
    id: string,
    body: ProjectItemInspectionInput,
  ): Observable<ProjectItemInspection> {
    return this.http.put<ProjectItemInspection>(
      `${API_BASE_URL}/api/project-items/${id}/inspection`,
      body,
    );
  }

  calculate(
    projectId: string,
    body: CalculateProjectBody = {},
  ): Observable<CalculateProjectResult> {
    return this.http.post<CalculateProjectResult>(
      `${API_BASE_URL}/api/projects/${projectId}/calculate`,
      body,
    );
  }
}
