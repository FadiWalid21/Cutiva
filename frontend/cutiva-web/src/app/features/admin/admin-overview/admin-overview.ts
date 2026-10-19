import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AdminApi, AdminReportSummary } from '../../../core/api/admin.api';

@Component({
  selector: 'app-admin-overview',
  imports: [TranslatePipe, RouterLink],
  templateUrl: './admin-overview.html',
  styleUrl: './admin-overview.scss',
})
export class AdminOverview implements OnInit {
  private readonly adminApi = inject(AdminApi);

  readonly summary = signal<AdminReportSummary | null>(null);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.adminApi.reportSummary().subscribe({
      next: (s) => this.summary.set(s),
      error: () => this.error.set('common.loadError'),
    });
  }
}
