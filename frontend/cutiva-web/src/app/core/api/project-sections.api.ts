import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

import { SectionAccessory } from './accessories.api';

export interface ProjectSection {
  id: string;
  projectId: string;
  sectorId: string;
  sectorCode: string;
  sectorName: string;
  name: string;
  heightMm: number | null;
  widthMm: number | null;
  unitQuantity: number | null;
  accessories: SectionAccessory[];
}

export interface CreateProjectSectionBody {
  sectorId: string;
  name: string;
  heightMm: number;
  widthMm: number;
  unitQuantity: number;
  accessories?: { accessoryId: string; quantity: number }[];
}

@Injectable({ providedIn: 'root' })
export class ProjectSectionsApi {
  private readonly http = inject(HttpClient);

  private base(projectId: string): string {
    return `${API_BASE_URL}/api/projects/${projectId}/sections`;
  }

  list(projectId: string): Observable<ProjectSection[]> {
    return this.http.get<ProjectSection[]>(this.base(projectId));
  }

  create(
    projectId: string,
    body: CreateProjectSectionBody,
  ): Observable<{
    id: string;
    projectId: string;
    sectorId: string;
    name: string;
    heightMm: number;
    widthMm: number;
    unitQuantity: number;
  }> {
    return this.http.post<{
      id: string;
      projectId: string;
      sectorId: string;
      name: string;
      heightMm: number;
      widthMm: number;
      unitQuantity: number;
    }>(this.base(projectId), body);
  }

  delete(projectId: string, sectionId: string): Observable<void> {
    return this.http.delete<void>(`${this.base(projectId)}/${sectionId}`);
  }
}
