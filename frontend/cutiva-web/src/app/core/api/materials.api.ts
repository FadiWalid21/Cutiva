import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

/** Matches backend MaterialCategory (JsonStringEnumConverter). */
export type MaterialCategory = 'Aluminum' | 'Glass' | 'Accessory';

export interface Material {
  id: string;
  sectorId: string;
  sectorCode: string;
  sectorName: string;
  code: string;
  name: string;
  description: string | null;
  category: MaterialCategory;
  thicknessMm: number | null;
  isActive: boolean;
}

export interface CreateMaterialBody {
  sectorId: string;
  code: string;
  name: string;
  description?: string | null;
  category?: MaterialCategory;
  thicknessMm?: number | null;
}

export interface UpdateMaterialBody {
  name: string;
  description?: string | null;
  category?: MaterialCategory;
  thicknessMm?: number | null;
  isActive?: boolean;
}

@Injectable({ providedIn: 'root' })
export class MaterialsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/api/materials`;

  list(filters?: {
    sectorId?: string;
    category?: MaterialCategory;
    activeOnly?: boolean;
  }): Observable<Material[]> {
    let params = new HttpParams();
    if (filters?.sectorId) {
      params = params.set('sectorId', filters.sectorId);
    }
    if (filters?.category) {
      params = params.set('category', filters.category);
    }
    if (filters?.activeOnly !== undefined) {
      params = params.set('activeOnly', String(filters.activeOnly));
    }
    return this.http.get<Material[]>(this.baseUrl, { params });
  }

  getById(id: string): Observable<Material> {
    return this.http.get<Material>(`${this.baseUrl}/${id}`);
  }

  create(body: CreateMaterialBody): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.baseUrl, body);
  }

  update(id: string, body: UpdateMaterialBody): Observable<Material> {
    return this.http.put<Material>(`${this.baseUrl}/${id}`, body);
  }
}
