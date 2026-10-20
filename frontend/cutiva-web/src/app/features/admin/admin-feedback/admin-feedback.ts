import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { AdminApi, AdminWorkshopFeedback } from '../../../core/api/admin.api';

@Component({
  selector: 'app-admin-feedback',
  imports: [TranslatePipe, DatePipe],
  templateUrl: './admin-feedback.html',
  styleUrl: './admin-feedback.scss',
})
export class AdminFeedback implements OnInit {
  private readonly adminApi = inject(AdminApi);

  readonly items = signal<AdminWorkshopFeedback[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.error.set(null);
    this.adminApi.listFeedback(200).subscribe({
      next: (list) => {
        this.items.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('common.loadError');
      },
    });
  }

  stars(rating: number): string {
    const n = Math.max(0, Math.min(5, Math.round(rating)));
    return '★'.repeat(n) + '☆'.repeat(5 - n);
  }

  triggerKey(trigger: string): string {
    return trigger === 'AfterCalculate'
      ? 'admin.feedback.triggerCalculate'
      : 'admin.feedback.triggerLogin';
  }
}
