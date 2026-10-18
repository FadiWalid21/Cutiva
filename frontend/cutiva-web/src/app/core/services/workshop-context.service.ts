import { Injectable, computed, inject, signal } from '@angular/core';
import { catchError, of, tap } from 'rxjs';
import { WorkshopsApi } from '../api/workshops.api';
import { WorkshopSummary } from '../api/workshops.models';

const STORAGE_KEY = 'cutiva.workshopId';

@Injectable({ providedIn: 'root' })
export class WorkshopContextService {
  private readonly workshopsApi = inject(WorkshopsApi);

  private readonly workshopsSignal = signal<WorkshopSummary[]>([]);
  private readonly selectedIdSignal = signal<string | null>(null);
  private readonly loadingSignal = signal(false);
  private readonly errorSignal = signal<string | null>(null);
  private readonly loadedSignal = signal(false);

  readonly workshops = this.workshopsSignal.asReadonly();
  readonly selectedWorkshopId = this.selectedIdSignal.asReadonly();
  readonly loading = this.loadingSignal.asReadonly();
  readonly error = this.errorSignal.asReadonly();
  readonly loaded = this.loadedSignal.asReadonly();

  readonly selectedWorkshop = computed(() => {
    const id = this.selectedIdSignal();
    if (!id) return null;
    return this.workshopsSignal().find((w) => w.id === id) ?? null;
  });

  readonly hasWorkshops = computed(() => this.workshopsSignal().length > 0);

  load(): void {
    this.loadingSignal.set(true);
    this.errorSignal.set(null);

    this.workshopsApi
      .list(1, 50)
      .pipe(
        tap((page) => {
          this.workshopsSignal.set(page.items);
          this.resolveSelection(page.items);
          this.loadingSignal.set(false);
          this.loadedSignal.set(true);
        }),
        catchError(() => {
          this.workshopsSignal.set([]);
          this.selectedIdSignal.set(null);
          this.errorSignal.set('workshop.loadError');
          this.loadingSignal.set(false);
          this.loadedSignal.set(true);
          return of(null);
        }),
      )
      .subscribe();
  }

  selectWorkshop(id: string): void {
    const exists = this.workshopsSignal().some((w) => w.id === id);
    if (!exists) return;

    this.selectedIdSignal.set(id);
    localStorage.setItem(STORAGE_KEY, id);
  }

  reload(): void {
    this.load();
  }

  private resolveSelection(items: WorkshopSummary[]): void {
    if (items.length === 0) {
      this.selectedIdSignal.set(null);
      localStorage.removeItem(STORAGE_KEY);
      return;
    }

    const stored = localStorage.getItem(STORAGE_KEY);
    const match = stored ? items.find((w) => w.id === stored) : undefined;
    const selected = match ?? items[0];

    this.selectedIdSignal.set(selected.id);
    localStorage.setItem(STORAGE_KEY, selected.id);
  }
}
