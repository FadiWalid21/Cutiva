import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

/** Matches backend AccessoryUnit (JsonStringEnumConverter). */
export type AccessoryUnit = 'Piece' | 'Set' | 'Meter';

export interface Accessory {
  id: string;
  workshopId: string;
  name: string;
  brand: string | null;
  unit: AccessoryUnit;
  unitPrice: number;
  isActive: boolean;
  createdAtUtc: string;
}

export interface SectionAccessory {
  accessoryId: string;
  name: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface QuoteAccessoryLine {
  accessoryId: string;
  name: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface CreateAccessoryBody {
  name: string;
  unitPrice: number;
  brand?: string | null;
  unit?: AccessoryUnit;
}

export interface UpdateAccessoryBody {
  name: string;
  unitPrice: number;
  brand?: string | null;
  unit?: AccessoryUnit;
  isActive?: boolean;
}

@Injectable({ providedIn: 'root' })
export class AccessoriesApi {
  private readonly http = inject(HttpClient);

  listForWorkshop(
    workshopId: string,
    activeOnly = true,
  ): Observable<Accessory[]> {
    const params = new HttpParams().set('activeOnly', String(activeOnly));
    return this.http.get<Accessory[]>(
      `${API_BASE_URL}/api/workshops/${workshopId}/accessories`,
      { params },
    );
  }

  create(
    workshopId: string,
    body: CreateAccessoryBody,
  ): Observable<Accessory> {
    return this.http.post<Accessory>(
      `${API_BASE_URL}/api/workshops/${workshopId}/accessories`,
      body,
    );
  }

  update(id: string, body: UpdateAccessoryBody): Observable<Accessory> {
    return this.http.put<Accessory>(
      `${API_BASE_URL}/api/accessories/${id}`,
      body,
    );
  }

  delete(id: string): Observable<unknown> {
    return this.http.delete(`${API_BASE_URL}/api/accessories/${id}`);
  }
}
