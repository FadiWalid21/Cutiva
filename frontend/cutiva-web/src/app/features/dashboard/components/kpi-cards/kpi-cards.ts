import { DecimalPipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { DashboardKpis } from '../../data/dashboard.models';

@Component({
  selector: 'app-kpi-cards',
  imports: [TranslatePipe, DecimalPipe],
  templateUrl: './kpi-cards.html',
  styleUrl: './kpi-cards.scss',
})
export class KpiCards {
  readonly kpis = input.required<DashboardKpis>();
}
