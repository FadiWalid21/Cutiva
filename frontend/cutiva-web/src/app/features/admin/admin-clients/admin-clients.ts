import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import {
  AdminApi,
  AdminClientDetail,
  AdminClientListItem,
  AdminPlan,
  AdminWorkshopUser,
  PaymentStatus,
} from '../../../core/api/admin.api';

@Component({
  selector: 'app-admin-clients',
  imports: [FormsModule, TranslatePipe, DatePipe],
  templateUrl: './admin-clients.html',
  styleUrl: './admin-clients.scss',
})
export class AdminClients implements OnInit {
  private readonly adminApi = inject(AdminApi);

  readonly clients = signal<AdminClientListItem[]>([]);
  readonly plans = signal<AdminPlan[]>([]);
  readonly selected = signal<AdminClientDetail | null>(null);
  readonly users = signal<AdminWorkshopUser[]>([]);
  readonly error = signal<string | null>(null);
  readonly notice = signal<string | null>(null);
  readonly lastTempPassword = signal<string | null>(null);

  workshopName = '';
  ownerEmail = '';
  ownerDisplayName = '';
  planCode = 'Starter';
  durationDays = 30;
  paymentStatus: PaymentStatus = 'Pending';
  paymentAmount: number | null = null;
  paymentNote = '';

  assignPlanCode = 'Workshop';
  assignDays = 30;
  assignPaymentStatus: PaymentStatus = 'Confirmed';
  assignPaymentAmount: number | null = null;
  assignPaymentNote = '';

  newUserEmail = '';
  newUserName = '';

  ngOnInit(): void {
    this.reload();
    this.adminApi.listPlans().subscribe({
      next: (plans) => {
        this.plans.set(plans);
        if (plans.length) {
          this.planCode = plans[0].code;
          this.assignPlanCode = plans[1]?.code ?? plans[0].code;
          this.paymentAmount = plans[0].priceSar;
        }
      },
    });
  }

  reload(): void {
    this.adminApi.listClients().subscribe({
      next: (items) => this.clients.set(items),
      error: () => this.error.set('common.loadError'),
    });
  }

  openClient(client: AdminClientListItem): void {
    this.error.set(null);
    this.adminApi.getClient(client.workshopId).subscribe({
      next: (detail) => {
        this.selected.set(detail);
        this.users.set(detail.users ?? []);
        this.assignPlanCode = detail.planCode ?? this.assignPlanCode;
        this.assignPaymentStatus =
          (detail.paymentStatus as PaymentStatus) || 'Pending';
        this.assignPaymentAmount = detail.paymentAmount;
        this.assignPaymentNote = detail.paymentNote ?? '';
      },
      error: () => this.error.set('common.loadError'),
    });
  }

  createClient(): void {
    this.error.set(null);
    this.notice.set(null);
    this.lastTempPassword.set(null);
    this.adminApi
      .createClient({
        workshopName: this.workshopName.trim(),
        ownerEmail: this.ownerEmail.trim(),
        ownerDisplayName: this.ownerDisplayName.trim() || null,
        planCode: this.planCode,
        durationDays: Number(this.durationDays) || 30,
        paymentStatus: this.paymentStatus,
        paymentAmount: this.paymentAmount,
        paymentCurrency: 'SAR',
        paymentNote: this.paymentNote.trim() || null,
      })
      .subscribe({
        next: (result) => {
          this.workshopName = '';
          this.ownerEmail = '';
          this.ownerDisplayName = '';
          this.paymentNote = '';
          this.lastTempPassword.set(result.temporaryPassword);
          this.notice.set(
            result.emailSent ? 'admin.clientCreatedEmail' : 'admin.clientCreatedNoEmail',
          );
          this.reload();
          if (result.client) {
            this.selected.set(result.client);
            this.users.set(result.client.users ?? []);
          }
        },
        error: () => this.error.set('common.saveError'),
      });
  }

  assignPlan(): void {
    const client = this.selected();
    if (!client) return;
    this.adminApi
      .upsertSubscription(client.workshopId, {
        planCode: this.assignPlanCode,
        durationDays: Number(this.assignDays) || 30,
        paymentStatus: this.assignPaymentStatus,
        paymentAmount: this.assignPaymentAmount,
        paymentCurrency: 'SAR',
        paymentNote: this.assignPaymentNote.trim() || null,
      })
      .subscribe({
        next: (detail) => {
          this.selected.set(detail);
          this.notice.set('admin.subscriptionUpdated');
          this.reload();
        },
        error: () => this.error.set('admin.seatConflict'),
      });
  }

  suspend(): void {
    const client = this.selected();
    if (!client) return;
    this.adminApi.suspendSubscription(client.workshopId).subscribe({
      next: (detail) => {
        this.selected.set(detail);
        this.notice.set('admin.subscriptionSuspended');
        this.reload();
      },
      error: () => this.error.set('common.saveError'),
    });
  }

  addUser(): void {
    const client = this.selected();
    if (!client || !this.newUserEmail.trim()) return;
    this.lastTempPassword.set(null);
    this.adminApi
      .addUser(client.workshopId, {
        email: this.newUserEmail.trim(),
        displayName: this.newUserName.trim() || null,
      })
      .subscribe({
        next: (result) => {
          this.newUserEmail = '';
          this.newUserName = '';
          this.lastTempPassword.set(result.temporaryPassword);
          this.notice.set(
            result.emailSent ? 'admin.userAddedEmail' : 'admin.userAddedNoEmail',
          );
          this.openClient(client);
          this.reload();
        },
        error: () => this.error.set('admin.seatConflict'),
      });
  }

  resetUser(user: AdminWorkshopUser): void {
    const client = this.selected();
    if (!client) return;
    this.lastTempPassword.set(null);
    this.adminApi.resetUserPassword(client.workshopId, user.userId).subscribe({
      next: (result) => {
        this.lastTempPassword.set(result.temporaryPassword);
        this.notice.set(
          result.emailSent ? 'admin.passwordResetEmail' : 'admin.passwordResetNoEmail',
        );
      },
      error: () => this.error.set('common.saveError'),
    });
  }

  toggleLock(user: AdminWorkshopUser): void {
    const client = this.selected();
    if (!client) return;
    const op = user.isLockedOut
      ? this.adminApi.unlockUser(client.workshopId, user.userId)
      : this.adminApi.lockUser(client.workshopId, user.userId);
    op.subscribe({
      next: () => this.openClient(client),
      error: () => this.error.set('common.saveError'),
    });
  }

  seatLabel(client: AdminClientListItem | AdminClientDetail): string {
    if (client.maxUsers == null) return `${client.seatUsed}/∞`;
    return `${client.seatUsed}/${client.maxUsers}`;
  }
}
