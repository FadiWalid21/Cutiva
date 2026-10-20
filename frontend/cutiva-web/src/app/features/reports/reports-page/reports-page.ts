import { DecimalPipe, DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { WorkshopsApi } from '../../../core/api/workshops.api';
import { WorkshopReportResponse } from '../../../core/api/workshops.models';
import { WorkshopContextService } from '../../../core/services/workshop-context.service';

@Component({
  selector: 'app-reports-page',
  imports: [FormsModule, TranslatePipe, DecimalPipe, DatePipe, RouterLink],
  templateUrl: './reports-page.html',
  styleUrl: './reports-page.scss',
})
export class ReportsPage implements OnInit {
  private readonly workshopsApi = inject(WorkshopsApi);
  readonly workshopContext = inject(WorkshopContextService);

  readonly report = signal<WorkshopReportResponse | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  fromDate = '';
  toDate = '';

  ngOnInit(): void {
    const to = new Date();
    const from = new Date();
    from.setDate(to.getDate() - 30);
    this.toDate = to.toISOString().slice(0, 10);
    this.fromDate = from.toISOString().slice(0, 10);
    this.reload();
  }

  reload(): void {
    const workshopId = this.workshopContext.selectedWorkshopId();
    if (!workshopId) {
      this.error.set('dashboard.noWorkshop');
      this.report.set(null);
      return;
    }

    const from = new Date(`${this.fromDate}T00:00:00.000Z`).toISOString();
    const to = new Date(`${this.toDate}T23:59:59.999Z`).toISOString();

    this.loading.set(true);
    this.error.set(null);
    this.workshopsApi.getReportSummary(workshopId, from, to).subscribe({
      next: (data) => {
        this.loading.set(false);
        this.report.set(data);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('reports.loadError');
      },
    });
  }
}
