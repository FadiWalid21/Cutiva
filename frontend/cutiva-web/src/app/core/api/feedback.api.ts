import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export type FeedbackTrigger = 'AfterCalculate' | 'LoginIdle';

export interface SubmitFeedbackBody {
  rating: number;
  comment?: string | null;
  trigger: FeedbackTrigger;
  workshopId?: string | null;
}

export interface WorkshopFeedbackDto {
  id: string;
  userId: string;
  workshopId: string | null;
  ownerDisplayName: string;
  companyName: string;
  rating: number;
  comment: string | null;
  trigger: FeedbackTrigger;
  createdAtUtc: string;
}

@Injectable({ providedIn: 'root' })
export class FeedbackApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/api/feedback`;

  submit(body: SubmitFeedbackBody): Observable<WorkshopFeedbackDto> {
    return this.http.post<WorkshopFeedbackDto>(this.baseUrl, body);
  }
}
