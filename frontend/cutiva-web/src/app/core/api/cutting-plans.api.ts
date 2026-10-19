import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export type CuttingPlanStatus = 'Draft' | 'Approved' | string;

export interface CuttingPlanPiece {
  requiredCutId: string;
  name: string;
  lengthMm: number;
}

export interface CuttingPlanBar {
  stockBarId: string;
  lengthMm: number;
  unitPrice: number;
  kerfMm: number;
  remainingLengthMm: number;
  isRemnant: boolean;
  pieces: CuttingPlanPiece[];
}

export interface CuttingPlan {
  cuttingPlanId: string;
  workshopId: string;
  materialId: string;
  projectId: string | null;
  comparisonGroupId: string | null;
  strategyCode: string;
  kerfMm: number;
  useRemnants: boolean;
  isComplete: boolean;
  status?: CuttingPlanStatus;
  approvedAtUtc?: string | null;
  totalPieceLengthMm: number;
  totalStockLengthMm: number;
  totalBarsUsed: number;
  totalMaterialCost: number;
  totalKerfMm: number;
  totalLeftoverMm: number;
  createdAtUtc: string;
  bars: CuttingPlanBar[];
  unallocatedPieces: CuttingPlanPiece[];
  strategyNote: string;
}

export interface CuttingPlanAlternativeSummary {
  cuttingPlanId: string;
  strategyCode: string;
  isComplete: boolean;
  totalBarsUsed: number;
  totalMaterialCost: number;
  totalPieceLengthMm: number;
  totalStockLengthMm: number;
  totalLeftoverMm: number;
  wastePercent: number;
  barsSummary: string;
  strategyNote: string;
}

export interface CuttingPlanComparison {
  comparisonGroupId: string;
  recommendedCostPlanId: string | null;
  recommendedWastePlanId: string | null;
  alternatives: CuttingPlanAlternativeSummary[];
}

export interface GenerateCuttingPlanBody {
  materialId: string;
  kerfMm: number;
  requiredCutIds: string[];
  /** Always send true|false so the API never falls back unexpectedly. */
  useRemnants: boolean;
}

@Injectable({ providedIn: 'root' })
export class CuttingPlansApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/api/cutting-plans`;

  generate(body: GenerateCuttingPlanBody): Observable<CuttingPlanComparison> {
    return this.http.post<CuttingPlanComparison>(this.baseUrl, body);
  }

  getComparison(groupId: string): Observable<CuttingPlanComparison> {
    return this.http.get<CuttingPlanComparison>(
      `${this.baseUrl}/comparisons/${groupId}`,
    );
  }

  getById(id: string): Observable<CuttingPlan> {
    return this.http.get<CuttingPlan>(`${this.baseUrl}/${id}`);
  }

  approve(id: string): Observable<CuttingPlan> {
    return this.http.post<CuttingPlan>(`${this.baseUrl}/${id}/approve`, {});
  }
}
