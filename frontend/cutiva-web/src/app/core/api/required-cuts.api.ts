import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export interface RequiredCut {
  id: string;
  projectSectionId: string;
  projectSectionName: string;
  materialId: string;
  materialCode: string;
  materialName: string;
  name: string;
  lengthMm: number;
  quantity: number;
  totalLengthMm: number;
}

export interface CreateRequiredCutBody {
  projectSectionId: string;
  materialId: string;
  name: string;
  lengthMm: number;
  quantity: number;
}

@Injectable({ providedIn: 'root' })
export class RequiredCutsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/api/required-cuts`;

  list(filters?: {
    projectSectionId?: string;
    materialId?: string;
  }): Observable<RequiredCut[]> {
    let params = new HttpParams();
    if (filters?.projectSectionId) {
      params = params.set('projectSectionId', filters.projectSectionId);
    }
    if (filters?.materialId) {
      params = params.set('materialId', filters.materialId);
    }
    return this.http.get<RequiredCut[]>(this.baseUrl, { params });
  }

  create(body: CreateRequiredCutBody): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.baseUrl, body);
  }

  update(id: string, body: CreateRequiredCutBody): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, body);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
