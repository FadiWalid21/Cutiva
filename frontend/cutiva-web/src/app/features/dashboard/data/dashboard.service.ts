import { Injectable, inject } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { WorkshopsApi } from '../../../core/api/workshops.api';
import { environment } from '../../../../environments/environment';
import { mapWorkshopDashboard } from './dashboard.mapper';
import { DASHBOARD_MOCK } from './dashboard.mock';
import { DashboardSnapshot } from './dashboard.models';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly workshopsApi = inject(WorkshopsApi);

  getSnapshot(workshopId: string): Observable<DashboardSnapshot> {
    if (environment.useDashboardMock) {
      return of(DASHBOARD_MOCK);
    }

    return this.workshopsApi
      .getDashboard(workshopId)
      .pipe(map(mapWorkshopDashboard));
  }
}
