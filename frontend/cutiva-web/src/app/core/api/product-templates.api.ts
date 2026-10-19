import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export type ProductKind = 'Window' | 'Door';
export type TemplatePartRole =
  | 'FrameTop'
  | 'FrameBottom'
  | 'FrameJamb'
  | 'SashStile'
  | 'SashRail'
  | 'InterlockStile';
export type AccessoryQuantityRule = 'PerProduct' | 'PerSash';

export interface ProductVariantSummary {
  id: string;
  name: string;
  code: string;
  sashCount: number | null;
  overlapMm: number | null;
  isActive: boolean;
  isSeeded: boolean;
  partCount: number;
  glassSlotCount: number;
  accessorySlotCount: number;
}

export interface ProductType {
  id: string;
  kind: ProductKind;
  name: string;
  isActive: boolean;
  isSeeded: boolean;
  variants: ProductVariantSummary[];
}

export interface TemplatePart {
  id: string;
  productVariantId: string;
  role: TemplatePartRole;
  displayName: string;
  qtyPerUnit: number;
  widthCoef: number;
  heightCoef: number;
  offsetMm: number;
  useSashWidthDivision: boolean;
  defaultAluminumMaterialId: string | null;
}

export interface TemplateGlassSlot {
  id: string;
  productVariantId: string;
  name: string;
  paneCount: number;
  widthCoefW: number;
  heightCoefW: number;
  offsetWMm: number;
  widthCoefH: number;
  heightCoefH: number;
  offsetHMm: number;
  required: boolean;
  useSashWidthDivision: boolean;
}

export interface TemplateAccessorySlot {
  id: string;
  productVariantId: string;
  name: string;
  quantityRule: AccessoryQuantityRule;
  qty: number;
  defaultAccessoryId: string | null;
}

export interface ProductVariant {
  id: string;
  productTypeId: string;
  productKind: ProductKind;
  productTypeName: string;
  name: string;
  code: string;
  sashCount: number | null;
  overlapMm: number | null;
  isActive: boolean;
  isSeeded: boolean;
  parts: TemplatePart[];
  glassSlots: TemplateGlassSlot[];
  accessorySlots: TemplateAccessorySlot[];
}

export interface UpdateProductVariantBody {
  name: string;
  sashCount?: number | null;
  overlapMm?: number | null;
  isActive?: boolean;
}

export interface UpdateTemplatePartBody {
  displayName: string;
  qtyPerUnit: number;
  widthCoef: number;
  heightCoef: number;
  offsetMm: number;
  useSashWidthDivision: boolean;
  defaultAluminumMaterialId?: string | null;
}

export interface UpdateTemplateGlassSlotBody {
  name: string;
  paneCount: number;
  widthCoefW: number;
  heightCoefW: number;
  offsetWMm: number;
  widthCoefH: number;
  heightCoefH: number;
  offsetHMm: number;
  required: boolean;
  useSashWidthDivision: boolean;
}

export interface UpdateTemplateAccessorySlotBody {
  name: string;
  quantityRule: AccessoryQuantityRule;
  qty: number;
  defaultAccessoryId?: string | null;
}

@Injectable({ providedIn: 'root' })
export class ProductTemplatesApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/api/product-templates`;

  listTypes(activeOnly = false): Observable<ProductType[]> {
    const params = new HttpParams().set('activeOnly', String(activeOnly));
    return this.http.get<ProductType[]>(`${this.baseUrl}/types`, { params });
  }

  getVariant(id: string): Observable<ProductVariant> {
    return this.http.get<ProductVariant>(`${this.baseUrl}/variants/${id}`);
  }

  updateVariant(
    id: string,
    body: UpdateProductVariantBody,
  ): Observable<ProductVariant> {
    return this.http.put<ProductVariant>(
      `${this.baseUrl}/variants/${id}`,
      body,
    );
  }

  updatePart(
    id: string,
    body: UpdateTemplatePartBody,
  ): Observable<TemplatePart> {
    return this.http.put<TemplatePart>(`${this.baseUrl}/parts/${id}`, body);
  }

  updateGlassSlot(
    id: string,
    body: UpdateTemplateGlassSlotBody,
  ): Observable<TemplateGlassSlot> {
    return this.http.put<TemplateGlassSlot>(
      `${this.baseUrl}/glass-slots/${id}`,
      body,
    );
  }

  updateAccessorySlot(
    id: string,
    body: UpdateTemplateAccessorySlotBody,
  ): Observable<TemplateAccessorySlot> {
    return this.http.put<TemplateAccessorySlot>(
      `${this.baseUrl}/accessory-slots/${id}`,
      body,
    );
  }
}
