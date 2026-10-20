import { AsyncPipe } from '@angular/common';
import {
  AfterViewInit,
  Component,
  ElementRef,
  inject,
  viewChild,
} from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { TranslatePipe } from '@ngx-translate/core';
import {
  Observable,
  catchError,
  combineLatest,
  map,
  of,
  startWith,
  switchMap,
} from 'rxjs';
import { MotionService } from '../../core/motion/motion.service';
import { PlanEntitlementsService } from '../../core/services/plan-entitlements.service';
import { WorkshopContextService } from '../../core/services/workshop-context.service';
import { KpiCards } from './components/kpi-cards/kpi-cards';
import { QuickCreateCta } from './components/quick-create-cta/quick-create-cta';
import { RecentProjects } from './components/recent-projects/recent-projects';
import { RemnantPanel } from './components/remnant-panel/remnant-panel';
import { DashboardSnapshot } from './data/dashboard.models';
import { DashboardService } from './data/dashboard.service';

export type DashboardViewState =
  | { status: 'boot' }
  | { status: 'noWorkshop' }
  | { status: 'loading' }
  | { status: 'ready'; data: DashboardSnapshot }
  | { status: 'error' };

@Component({
  selector: 'app-dashboard',
  imports: [
    AsyncPipe,
    TranslatePipe,
    KpiCards,
    RemnantPanel,
    RecentProjects,
    QuickCreateCta,
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements AfterViewInit {
  private readonly dashboardService = inject(DashboardService);
  private readonly workshopContext = inject(WorkshopContextService);
  private readonly motion = inject(MotionService);
  readonly entitlements = inject(PlanEntitlementsService);
  private readonly pageRoot = viewChild<ElementRef<HTMLElement>>('pageRoot');

  readonly state$: Observable<DashboardViewState> = combineLatest([
    toObservable(this.workshopContext.loaded),
    toObservable(this.workshopContext.selectedWorkshopId),
  ]).pipe(
    switchMap(([loaded, workshopId]) => {
      if (!loaded) {
        return of<DashboardViewState>({ status: 'boot' });
      }

      if (!workshopId) {
        return of<DashboardViewState>({ status: 'noWorkshop' });
      }

      return this.dashboardService.getSnapshot(workshopId).pipe(
        map(
          (data): DashboardViewState => ({
            status: 'ready',
            data,
          }),
        ),
        startWith<DashboardViewState>({ status: 'loading' }),
        catchError(() => of<DashboardViewState>({ status: 'error' })),
      );
    }),
  );

  ngAfterViewInit(): void {
    queueMicrotask(() => {
      this.motion.pageEnter(this.pageRoot()?.nativeElement ?? null);
    });
  }
}
