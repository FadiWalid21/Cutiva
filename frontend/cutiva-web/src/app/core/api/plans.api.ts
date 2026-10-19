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

export interface PricingDisplayPlan {
  code: string;
  name: string;
  price: number;
  maxUsers: number | null;
}

export interface PricingDisplay {
  countryCode: string | null;
  currency: string;
  rateSarToEgp: number | null;
  rateSource: string;
  plans: PricingDisplayPlan[];
}

@Injectable({ providedIn: 'root' })
export class PlansApi {
  private readonly http = inject(HttpClient);
  private readonly base = `${API_BASE_URL}/api/plans`;
  private readonly pricingBase = `${API_BASE_URL}/api/pricing`;

  list(): Observable<PublicPlan[]> {
    return this.http.get<PublicPlan[]>(this.base);
  }

  display(): Observable<PricingDisplay> {
    return this.http.get<PricingDisplay>(`${this.pricingBase}/display`);
  }
}
