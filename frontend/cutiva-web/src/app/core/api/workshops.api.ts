import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import {
  PagedWorkshopsResponse,
  WorkshopDashboardResponse,
  WorkshopReportResponse,
  WorkshopSummary,
} from './workshops.models';

@Injectable({ providedIn: 'root' })
export class WorkshopsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/api/workshops`;

  list(page = 1, pageSize = 50): Observable<PagedWorkshopsResponse> {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    return this.http.get<PagedWorkshopsResponse>(this.baseUrl, { params });
  }

  getById(id: string): Observable<WorkshopSummary> {
    return this.http.get<WorkshopSummary>(`${this.baseUrl}/${id}`);
  }

  getDashboard(workshopId: string): Observable<WorkshopDashboardResponse> {
    return this.http.get<WorkshopDashboardResponse>(
      `${this.baseUrl}/${workshopId}/dashboard`,
    );
  }

  getReportSummary(
    workshopId: string,
    from: string,
    to: string,
  ): Observable<WorkshopReportResponse> {
    const params = new HttpParams().set('from', from).set('to', to);
    return this.http.get<WorkshopReportResponse>(
      `${this.baseUrl}/${workshopId}/reports/summary`,
      { params },
    );
  }

  create(name: string): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.baseUrl, { name });
  }

  update(
    id: string,
    name: string,
    productionPricePerM2?: number | null,
  ): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, {
      name,
      productionPricePerM2: productionPricePerM2 ?? null,
    });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
