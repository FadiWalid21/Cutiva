import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export interface GlassSpec {
  id: string;
  glassTypeId: string;
  thicknessMm: number;
  description: string | null;
  pricePerSqm: number;
  isActive: boolean;
}

export interface GlassType {
  id: string;
  workshopId: string;
  name: string;
  isActive: boolean;
  specs: GlassSpec[];
}

export interface CreateGlassTypeBody {
  name: string;
}

export interface UpdateGlassTypeBody {
  name: string;
  isActive?: boolean;
}

export interface CreateGlassSpecBody {
  thicknessMm: number;
  pricePerSqm: number;
  description?: string | null;
}

export interface UpdateGlassSpecBody {
  thicknessMm: number;
  pricePerSqm: number;
  description?: string | null;
  isActive?: boolean;
}

@Injectable({ providedIn: 'root' })
export class GlassApi {
  private readonly http = inject(HttpClient);

  listTypes(
    workshopId: string,
    activeOnly = true,
  ): Observable<GlassType[]> {
    const params = new HttpParams().set('activeOnly', String(activeOnly));
    return this.http.get<GlassType[]>(
      `${API_BASE_URL}/api/workshops/${workshopId}/glass-types`,
      { params },
    );
  }

  createType(
    workshopId: string,
    body: CreateGlassTypeBody,
  ): Observable<GlassType> {
    return this.http.post<GlassType>(
      `${API_BASE_URL}/api/workshops/${workshopId}/glass-types`,
      body,
    );
  }

  updateType(id: string, body: UpdateGlassTypeBody): Observable<GlassType> {
    return this.http.put<GlassType>(
      `${API_BASE_URL}/api/glass-types/${id}`,
      body,
    );
  }

  createSpec(
    typeId: string,
    body: CreateGlassSpecBody,
  ): Observable<GlassSpec> {
    return this.http.post<GlassSpec>(
      `${API_BASE_URL}/api/glass-types/${typeId}/specs`,
      body,
    );
  }

  updateSpec(id: string, body: UpdateGlassSpecBody): Observable<GlassSpec> {
    return this.http.put<GlassSpec>(
      `${API_BASE_URL}/api/glass-specs/${id}`,
      body,
    );
  }

  getSpec(id: string): Observable<GlassSpec> {
    return this.http.get<GlassSpec>(`${API_BASE_URL}/api/glass-specs/${id}`);
  }
}
