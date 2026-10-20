import { DecimalPipe, DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import * as XLSX from 'xlsx';
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
  private readonly translate = inject(TranslateService);
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

  exportExcel(): void {
    const r = this.report();
    if (!r || this.loading()) return;

    const t = (key: string) => this.translate.instant(key);

    const summaryRows = [
      [t('reports.from'), this.fromDate],
      [t('reports.to'), this.toDate],
      [],
      [t('reports.kpi.projects'), r.kpis.projectsActive],
      [t('reports.kpi.plansGenerated'), r.kpis.plansGenerated],
      [t('reports.kpi.plansApproved'), r.kpis.plansApproved],
      [t('reports.kpi.barsUsed'), r.kpis.stockBarsUsed],
      [t('reports.kpi.efficiency'), r.kpis.materialEfficiencyPercent],
      [t('reports.kpi.waste'), r.kpis.wastePercent],
      [t('reports.kpi.cost'), r.kpis.totalMaterialCost],
      [t('reports.kpi.remnantsStock'), r.kpis.remnantBarsInStock],
      [t('reports.kpi.remnantsUsed'), r.kpis.remnantBarsConsumed],
    ];

    const materialsRows = [
      [
        t('reports.colMaterial'),
        t('reports.colPlans'),
        t('reports.colBars'),
        t('reports.colWaste'),
        t('reports.colCost'),
      ],
      ...r.topMaterialsByCost.map((row) => [
        row.materialName,
        row.plansApproved,
        row.stockBarsUsed,
        row.wastePercent,
        row.totalMaterialCost,
      ]),
    ];

    const projectsRows = [
      [
        t('reports.colProject'),
        t('reports.colGenerated'),
        t('reports.colApproved'),
        t('reports.colCost'),
        t('reports.colLast'),
      ],
      ...r.projects.map((row) => [
        row.projectName,
        row.plansGenerated,
        row.plansApproved,
        row.totalMaterialCost,
        row.lastActivityUtc.slice(0, 10),
      ]),
    ];

    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(
      workbook,
      XLSX.utils.aoa_to_sheet(summaryRows),
      'Summary',
    );
    XLSX.utils.book_append_sheet(
      workbook,
      XLSX.utils.aoa_to_sheet(materialsRows),
      'Materials',
    );
    XLSX.utils.book_append_sheet(
      workbook,
      XLSX.utils.aoa_to_sheet(projectsRows),
      'Projects',
    );

    const stamp = this.toDate || new Date().toISOString().slice(0, 10);
    XLSX.writeFile(workbook, `cutiva-workshop-report-${stamp}.xlsx`);
  }
}
