import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export interface StockBar {
  id: string;
  materialId: string;
  materialCode: string;
  materialName: string;
  sectorId: string;
  sectorCode: string;
  sectorName: string;
  lengthMm: number;
  unitPrice: number;
  availableQuantity: number;
  isActive: boolean;
}

export interface CreateStockBarBody {
  materialId: string;
  lengthMm: number;
  unitPrice: number;
  availableQuantity: number;
}

export interface UpdateStockBarBody {
  materialId: string;
  lengthMm: number;
  unitPrice: number;
  availableQuantity: number;
}

@Injectable({ providedIn: 'root' })
export class StockBarsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/api/stock-bars`;

  list(filters?: {
    materialId?: string;
    sectorId?: string;
    isActive?: boolean;
  }): Observable<StockBar[]> {
    let params = new HttpParams();
    if (filters?.materialId) {
      params = params.set('materialId', filters.materialId);
    }
    if (filters?.sectorId) {
      params = params.set('sectorId', filters.sectorId);
    }
    if (filters?.isActive !== undefined) {
      params = params.set('isActive', String(filters.isActive));
    }
    return this.http.get<StockBar[]>(this.baseUrl, { params });
  }

  create(body: CreateStockBarBody): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.baseUrl, body);
  }

  update(id: string, body: UpdateStockBarBody): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, body);
  }

  deactivate(id: string): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/deactivate`, {});
  }
}
