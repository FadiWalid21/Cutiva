import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export interface PublicPlan {
  code: string;
  name: string;
  priceSar: number;
  durationDays: number;
  maxUsers: number | null;
}

@Injectable({ providedIn: 'root' })
export class PlansApi {
  private readonly http = inject(HttpClient);
  private readonly base = `${API_BASE_URL}/api/plans`;

  list(): Observable<PublicPlan[]> {
    return this.http.get<PublicPlan[]>(this.base);
  }
}
