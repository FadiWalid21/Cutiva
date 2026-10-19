import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export interface SubscriptionSummary {
  planCode: string;
  planName: string;
  startsAtUtc: string;
  endsAtUtc: string;
  status: string;
  daysRemaining: number;
  maxUsers?: number | null;
  seatUsed?: number | null;
}

export interface AuthTokenResponse {
  accessToken: string;
  expiresAtUtc: string;
  email: string;
  displayName: string | null;
  roles: string[];
  workshopId: string | null;
  subscription: SubscriptionSummary | null;
}

export interface AuthMeResponse {
  userId: string;
  email: string;
  displayName: string | null;
  roles: string[];
  workshopId: string | null;
  subscription: SubscriptionSummary | null;
}

export interface LoginBody {
  email: string;
  password: string;
}

@Injectable({ providedIn: 'root' })
export class AuthApi {
  private readonly http = inject(HttpClient);
  private readonly base = `${API_BASE_URL}/api/auth`;

  login(body: LoginBody): Observable<AuthTokenResponse> {
    return this.http.post<AuthTokenResponse>(`${this.base}/login`, body);
  }

  me(): Observable<AuthMeResponse> {
    return this.http.get<AuthMeResponse>(`${this.base}/me`);
  }

  forgotPassword(email: string): Observable<void> {
    return this.http.post<void>(`${this.base}/forgot-password`, { email });
  }

  resetPassword(body: {
    email: string;
    code: string;
    newPassword: string;
  }): Observable<void> {
    return this.http.post<void>(`${this.base}/reset-password`, body);
  }
}
