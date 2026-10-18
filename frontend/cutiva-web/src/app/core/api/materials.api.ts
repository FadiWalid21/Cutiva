import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export interface Material {
  id: string;
  sectorId: string;
  sectorCode: string;
  sectorName: string;
  code: string;
  name: string;
  description: string | null;
}

export interface CreateMaterialBody {
  sectorId: string;
  code: string;
  name: string;
  description?: string | null;
}

@Injectable({ providedIn: 'root' })
export class MaterialsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/api/materials`;

  list(sectorId?: string): Observable<Material[]> {
    let params = new HttpParams();
    if (sectorId) {
      params = params.set('sectorId', sectorId);
    }
    return this.http.get<Material[]>(this.baseUrl, { params });
  }

  getById(id: string): Observable<Material> {
    return this.http.get<Material>(`${this.baseUrl}/${id}`);
  }

  create(body: CreateMaterialBody): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.baseUrl, body);
  }
}
