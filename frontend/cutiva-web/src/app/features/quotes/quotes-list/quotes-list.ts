import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, effect, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { ClientQuoteSummary, QuotesApi } from '../../../core/api/quotes.api';
import { apiErrorMessage, isI18nKey } from '../../../core/http/api-error';
import { WorkshopContextService } from '../../../core/services/workshop-context.service';

@Component({
  selector: 'app-quotes-list',
  imports: [RouterLink, TranslatePipe, DecimalPipe, DatePipe],
  templateUrl: './quotes-list.html',
  styleUrl: './quotes-list.scss',
})
export class QuotesList {
  private readonly quotesApi = inject(QuotesApi);
  readonly workshopContext = inject(WorkshopContextService);

  readonly quotes = signal<ClientQuoteSummary[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly isI18nKey = isI18nKey;

  constructor() {
    effect(() => {
      const workshopId = this.workshopContext.selectedWorkshopId();
      const loaded = this.workshopContext.loaded();
      if (!loaded) return;
      if (!workshopId) {
        this.quotes.set([]);
        this.loading.set(false);
        this.error.set(null);
        return;
      }
      this.load(workshopId);
    });
  }

  private load(workshopId: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.quotesApi.listForWorkshop(workshopId).subscribe({
      next: (quotes) => {
        this.quotes.set(quotes);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(apiErrorMessage(err, 'common.loadError'));
        this.quotes.set([]);
        this.loading.set(false);
      },
    });
  }
}
