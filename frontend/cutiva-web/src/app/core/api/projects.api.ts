import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export interface ProjectListItem {
  id: string;
  workshopId: string;
  name: string;
  customerName: string | null;
}

export interface PagedProjects {
  items: ProjectListItem[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface CreateProjectBody {
  name: string;
  customerName?: string | null;
}

export interface CreatedProject {
  id: string;
  workshopId: string;
  name: string;
  customerName: string | null;
}

@Injectable({ providedIn: 'root' })
export class ProjectsApi {
  private readonly http = inject(HttpClient);

  private base(workshopId: string): string {
    return `${API_BASE_URL}/api/workshops/${workshopId}/projects`;
  }

  list(workshopId: string, page = 1, pageSize = 50): Observable<PagedProjects> {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    return this.http.get<PagedProjects>(this.base(workshopId), { params });
  }

  create(workshopId: string, body: CreateProjectBody): Observable<CreatedProject> {
    return this.http.post<CreatedProject>(this.base(workshopId), body);
  }

  update(
    workshopId: string,
    projectId: string,
    body: CreateProjectBody,
  ): Observable<void> {
    return this.http.put<void>(`${this.base(workshopId)}/${projectId}`, body);
  }

  delete(workshopId: string, projectId: string): Observable<void> {
    return this.http.delete<void>(`${this.base(workshopId)}/${projectId}`);
  }
}
