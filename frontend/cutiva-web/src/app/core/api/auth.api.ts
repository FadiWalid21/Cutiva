import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export interface AuthTokenResponse {
  accessToken: string;
  expiresAtUtc: string;
  email: string;
  displayName: string | null;
}

export interface AuthMeResponse {
  userId: string;
  email: string;
  displayName: string | null;
}

export interface RegisterBody {
  email: string;
  password: string;
  displayName?: string | null;
}

export interface LoginBody {
  email: string;
  password: string;
}

@Injectable({ providedIn: 'root' })
export class AuthApi {
  private readonly http = inject(HttpClient);
  private readonly base = `${API_BASE_URL}/api/auth`;

  register(body: RegisterBody): Observable<AuthTokenResponse> {
    return this.http.post<AuthTokenResponse>(`${this.base}/register`, body);
  }

  login(body: LoginBody): Observable<AuthTokenResponse> {
    return this.http.post<AuthTokenResponse>(`${this.base}/login`, body);
  }

  me(): Observable<AuthMeResponse> {
    return this.http.get<AuthMeResponse>(`${this.base}/me`);
  }
}
