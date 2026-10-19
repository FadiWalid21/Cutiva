import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export interface QuoteSectionPreview {
  sectionId: string;
  name: string;
  sectorCode: string;
  sectorName: string;
  heightMm: number | null;
  widthMm: number | null;
  unitQuantity: number | null;
}

export interface QuoteSurcharge {
  id: string;
  reason: string;
  amount: number;
}

export interface QuoteAccessoryLine {
  accessoryId: string;
  name: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface QuoteGlassLine {
  glassSpecId: string;
  name: string;
  widthMm: number;
  heightMm: number;
  areaM2PerPane: number;
  paneQuantity: number;
  pricePerSqm: number;
  lineTotal: number;
}

export interface ClientQuote {
  id: string;
  projectId: string;
  projectName: string;
  customerName: string | null;
  cuttingPlanId: string | null;
  workshopId: string;
  workshopName: string;
  materialCode: string;
  materialName: string;
  materialCost: number;
  productionAreaM2: number;
  productionPricePerM2: number | null;
  productionCost: number;
  accessoriesTotal: number;
  glassTotal: number;
  productAluminumTotal: number;
  surchargesTotal: number;
  grandTotal: number;
  notes: string | null;
  createdAtUtc: string;
  surcharges: QuoteSurcharge[];
  accessories: QuoteAccessoryLine[];
  glassLines: QuoteGlassLine[];
  sections: QuoteSectionPreview[];
}

export interface ClientQuoteSummary {
  id: string;
  projectId: string;
  projectName: string;
  customerName: string | null;
  cuttingPlanId: string | null;
  materialName: string;
  grandTotal: number;
  createdAtUtc: string;
}

export interface UpsertQuoteBody {
  projectId: string;
  cuttingPlanId?: string | null;
  notes?: string | null;
  surcharges: { reason: string; amount: number }[];
}

@Injectable({ providedIn: 'root' })
export class QuotesApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/api/quotes`;

  listForWorkshop(workshopId: string): Observable<ClientQuoteSummary[]> {
    return this.http.get<ClientQuoteSummary[]>(
      `${API_BASE_URL}/api/workshops/${workshopId}/quotes`,
    );
  }

  upsert(body: UpsertQuoteBody): Observable<ClientQuote> {
    return this.http.post<ClientQuote>(this.baseUrl, body);
  }

  getById(id: string): Observable<ClientQuote> {
    return this.http.get<ClientQuote>(`${this.baseUrl}/${id}`);
  }

  getByPlan(projectId: string, cuttingPlanId: string): Observable<ClientQuote> {
    const params = new HttpParams()
      .set('projectId', projectId)
      .set('cuttingPlanId', cuttingPlanId);
    return this.http.get<ClientQuote>(this.baseUrl, { params });
  }
}
