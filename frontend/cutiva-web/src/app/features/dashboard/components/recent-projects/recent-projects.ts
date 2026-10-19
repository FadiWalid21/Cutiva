import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { RecentProject } from '../../data/dashboard.models';

@Component({
  selector: 'app-recent-projects',
  imports: [TranslatePipe, DecimalPipe, DatePipe, RouterLink],
  templateUrl: './recent-projects.html',
  styleUrl: './recent-projects.scss',
})
export class RecentProjects {
  readonly projects = input.required<RecentProject[]>();

  statusKey(status: RecentProject['status']): string {
    return `dashboard.status.${status}`;
  }

  statusClass(status: RecentProject['status']): string {
    if (status === 'optimized') return 'badge badge--success';
    if (status === 'incomplete') return 'badge badge--warning';
    return 'badge badge--info';
  }
}
