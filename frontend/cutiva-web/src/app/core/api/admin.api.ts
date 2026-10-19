import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export interface AdminWorkshopUser {
  userId: string;
  email: string;
  displayName: string | null;
  isPrimaryContact: boolean;
  roles: string[];
  isLockedOut: boolean;
}

export interface AdminClientListItem {
  workshopId: string;
  workshopName: string;
  ownerUserId: string | null;
  ownerEmail: string | null;
  ownerDisplayName: string | null;
  planCode: string | null;
  planName: string | null;
  maxUsers: number | null;
  seatUsed: number;
  subscriptionStatus: string | null;
  paymentStatus: string | null;
  endsAtUtc: string | null;
  daysRemaining: number | null;
  projectCount: number;
}

export interface AdminClientDetail extends AdminClientListItem {
  productionPricePerM2: number | null;
  paymentAmount: number | null;
  paymentCurrency: string | null;
  paymentNote: string | null;
  paymentConfirmedAtUtc: string | null;
  startsAtUtc: string | null;
  quoteCount: number;
  users: AdminWorkshopUser[];
}

export interface AdminPlan {
  id: string;
  code: string;
  name: string;
  priceSar: number;
  durationDays: number;
  maxUsers: number | null;
  isActive: boolean;
}

export interface AdminReportSummary {
  totalClients: number;
  activeSubscriptions: number;
  expiredSubscriptions: number;
  suspendedSubscriptions: number;
  expiringIn7Days: number;
  byPlan: { planCode: string; planName: string; count: number }[];
}

export type PaymentStatus = 'Pending' | 'Confirmed' | 'Waived';

export interface CreateAdminClientBody {
  workshopName: string;
  ownerEmail: string;
  ownerPassword?: string | null;
  ownerDisplayName?: string | null;
  planCode: string;
  durationDays?: number | null;
  paymentStatus?: PaymentStatus | null;
  paymentAmount?: number | null;
  paymentCurrency?: string | null;
  paymentNote?: string | null;
}

export interface UpsertSubscriptionBody {
  planCode: string;
  durationDays?: number | null;
  endsAtUtc?: string | null;
  paymentStatus?: PaymentStatus | null;
  paymentAmount?: number | null;
  paymentCurrency?: string | null;
  paymentNote?: string | null;
}

export interface AddWorkshopUserResult {
  user: AdminWorkshopUser;
  temporaryPassword: string;
  emailSent: boolean;
}

export interface AdminTempPasswordResult {
  temporaryPassword: string;
  emailSent: boolean;
}

@Injectable({ providedIn: 'root' })
export class AdminApi {
  private readonly http = inject(HttpClient);
  private readonly base = `${API_BASE_URL}/api/admin`;

  listClients(): Observable<AdminClientListItem[]> {
    return this.http.get<AdminClientListItem[]>(`${this.base}/clients`);
  }

  getClient(workshopId: string): Observable<AdminClientDetail> {
    return this.http.get<AdminClientDetail>(`${this.base}/clients/${workshopId}`);
  }

  createClient(body: CreateAdminClientBody): Observable<{
    client: AdminClientDetail;
    temporaryPassword: string;
    emailSent: boolean;
  }> {
    return this.http.post<{
      client: AdminClientDetail;
      temporaryPassword: string;
      emailSent: boolean;
    }>(`${this.base}/clients`, body);
  }

  updateClient(
    workshopId: string,
    body: { workshopName: string; ownerDisplayName?: string | null },
  ): Observable<AdminClientDetail> {
    return this.http.put<AdminClientDetail>(
      `${this.base}/clients/${workshopId}`,
      body,
    );
  }

  upsertSubscription(
    workshopId: string,
    body: UpsertSubscriptionBody,
  ): Observable<AdminClientDetail> {
    return this.http.put<AdminClientDetail>(
      `${this.base}/clients/${workshopId}/subscription`,
      body,
    );
  }

  suspendSubscription(workshopId: string): Observable<AdminClientDetail> {
    return this.http.delete<AdminClientDetail>(
      `${this.base}/clients/${workshopId}/subscription`,
    );
  }

  listUsers(workshopId: string): Observable<AdminWorkshopUser[]> {
    return this.http.get<AdminWorkshopUser[]>(
      `${this.base}/clients/${workshopId}/users`,
    );
  }

  addUser(
    workshopId: string,
    body: { email: string; displayName?: string | null; password?: string | null },
  ): Observable<AddWorkshopUserResult> {
    return this.http.post<AddWorkshopUserResult>(
      `${this.base}/clients/${workshopId}/users`,
      body,
    );
  }

  resetUserPassword(
    workshopId: string,
    userId: string,
    newPassword?: string | null,
  ): Observable<AdminTempPasswordResult> {
    return this.http.post<AdminTempPasswordResult>(
      `${this.base}/clients/${workshopId}/users/${userId}/reset-password`,
      { newPassword: newPassword ?? null },
    );
  }

  lockUser(workshopId: string, userId: string): Observable<void> {
    return this.http.post<void>(
      `${this.base}/clients/${workshopId}/users/${userId}/lock`,
      {},
    );
  }

  unlockUser(workshopId: string, userId: string): Observable<void> {
    return this.http.post<void>(
      `${this.base}/clients/${workshopId}/users/${userId}/unlock`,
      {},
    );
  }

  listPlans(): Observable<AdminPlan[]> {
    return this.http.get<AdminPlan[]>(`${this.base}/plans`);
  }

  updatePlan(
    planId: string,
    body: {
      name: string;
      priceSar: number;
      maxUsers: number | null;
      durationDays?: number | null;
      isActive?: boolean | null;
    },
  ): Observable<AdminPlan> {
    return this.http.put<AdminPlan>(`${this.base}/plans/${planId}`, body);
  }

  reportSummary(): Observable<AdminReportSummary> {
    return this.http.get<AdminReportSummary>(`${this.base}/reports/summary`);
  }
}
