import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export interface Sector {
  id: string;
  code: string;
  name: string;
}

export interface CreateSectorBody {
  code: string;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class SectorsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/api/sectors`;

  list(): Observable<Sector[]> {
    return this.http.get<Sector[]>(this.baseUrl);
  }

  create(body: CreateSectorBody): Observable<{ id: string; code: string; name: string }> {
    return this.http.post<{ id: string; code: string; name: string }>(
      this.baseUrl,
      body,
    );
  }

  update(id: string, body: CreateSectorBody): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, body);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
