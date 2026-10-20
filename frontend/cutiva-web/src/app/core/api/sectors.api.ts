import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, of, switchMap, throwError } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import { MARKET_MATERIALS_SECTOR_CODE } from '../constants/market';

export interface Sector {
  id: string;
  code: string;
  name: string;
  materialId: string | null;
  materialCode: string | null;
  materialName: string | null;
}

export interface CreateSectorBody {
  code: string;
  name: string;
  materialId?: string | null;
}

@Injectable({ providedIn: 'root' })
export class SectorsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/api/sectors`;

  list(): Observable<Sector[]> {
    return this.http.get<Sector[]>(this.baseUrl);
  }

  create(body: CreateSectorBody): Observable<{
    id: string;
    code: string;
    name: string;
    materialId: string | null;
  }> {
    return this.http.post<{
      id: string;
      code: string;
      name: string;
      materialId: string | null;
    }>(this.baseUrl, body);
  }

  update(id: string, body: CreateSectorBody): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, body);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  /**
   * Returns the shared MAT sector, creating it when missing. A 409 means another
   * request created it first, so the list is re-read instead of failing.
   */
  ensureMarketSector(name: string): Observable<Sector> {
    const findMarket = (sectors: Sector[]) =>
      sectors.find((s) => s.code.toUpperCase() === MARKET_MATERIALS_SECTOR_CODE);

    return this.list().pipe(
      switchMap((sectors) => {
        const existing = findMarket(sectors);
        if (existing) return of(existing);

        return this.create({
          code: MARKET_MATERIALS_SECTOR_CODE,
          name,
          materialId: null,
        }).pipe(
          map(
            (created): Sector => ({
              id: created.id,
              code: created.code,
              name: created.name,
              materialId: created.materialId,
              materialCode: null,
              materialName: null,
            }),
          ),
          catchError((err: unknown) => {
            if (!(err instanceof HttpErrorResponse) || err.status !== 409) {
              return throwError(() => err);
            }
            return this.list().pipe(
              switchMap((retry) => {
                const found = findMarket(retry);
                return found ? of(found) : throwError(() => err);
              }),
            );
          }),
        );
      }),
    );
  }
}
