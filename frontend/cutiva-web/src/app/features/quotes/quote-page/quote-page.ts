import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { catchError, of, switchMap } from 'rxjs';
import { ClientQuote, QuotesApi } from '../../../core/api/quotes.api';
import { apiErrorMessage, isI18nKey } from '../../../core/http/api-error';
import { formatCm } from '../../../core/units/length';
import { AluminumFinishBar } from '../../../shared/aluminum-finish-bar/aluminum-finish-bar';
import { FramePreview } from '../../../shared/frame-preview/frame-preview';

interface SurchargeDraft {
  reason: string;
  amount: number | null;
}

@Component({
  selector: 'app-quote-page',
  imports: [
    FormsModule,
    RouterLink,
    TranslatePipe,
    DecimalPipe,
    DatePipe,
    FramePreview,
    AluminumFinishBar,
  ],
  templateUrl: './quote-page.html',
  styleUrl: './quote-page.scss',
})
export class QuotePage {
  private readonly route = inject(ActivatedRoute);
  private readonly quotesApi = inject(QuotesApi);

  readonly quote = signal<ClientQuote | null>(null);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly notice = signal<string | null>(null);

  readonly isI18nKey = isI18nKey;
  readonly formatCm = formatCm;

  draftLines = signal<SurchargeDraft[]>([{ reason: '', amount: null }]);
  notes = '';

  readonly previewSections = computed(
    () => this.quote()?.sections.filter((s) => s.heightMm && s.widthMm) ?? [],
  );

  readonly draftSurchargeTotal = computed(() =>
    this.draftLines().reduce((sum, line) => sum + (Number(line.amount) || 0), 0),
  );

  readonly draftTotal = computed(() => {
    const q = this.quote();
    const material = q?.materialCost ?? 0;
    const production = q?.productionCost ?? 0;
    const accessories = q?.accessoriesTotal ?? 0;
    const glass = q?.glassTotal ?? 0;
    return (
      material + production + accessories + glass + this.draftSurchargeTotal()
    );
  });

  constructor() {
    this.route.paramMap
      .pipe(
        switchMap((params) => {
          const quoteId = params.get('quoteId');
          this.loading.set(true);
          this.error.set(null);

          if (!quoteId) {
            this.error.set('quote.notFound');
            return of(null);
          }

          return this.quotesApi.getById(quoteId).pipe(
            catchError((err) => {
              this.error.set(apiErrorMessage(err, 'quote.notFound'));
              return of(null);
            }),
          );
        }),
      )
      .subscribe((quote) => {
        this.applyQuote(quote);
        this.loading.set(false);
      });
  }

  private applyQuote(quote: ClientQuote | null): void {
    if (quote) {
      quote = {
        ...quote,
        accessories: quote.accessories ?? [],
        glassLines: quote.glassLines ?? [],
        glassTotal: quote.glassTotal ?? 0,
        productAluminumTotal: quote.productAluminumTotal ?? 0,
        productionAreaM2: quote.productionAreaM2 ?? 0,
        productionPricePerM2: quote.productionPricePerM2 ?? null,
        productionCost: quote.productionCost ?? 0,
      };
    }
    this.quote.set(quote);
    if (!quote) return;
    this.notes = quote.notes ?? '';
    this.draftLines.set(
      quote.surcharges.length > 0
        ? quote.surcharges.map((s) => ({
            reason: s.reason,
            amount: s.amount,
          }))
        : [{ reason: '', amount: null }],
    );
  }

  addLine(): void {
    this.draftLines.update((lines) => [...lines, { reason: '', amount: null }]);
  }

  removeLine(index: number): void {
    this.draftLines.update((lines) => {
      const next = lines.filter((_, i) => i !== index);
      return next.length ? next : [{ reason: '', amount: null }];
    });
  }

  updateReason(index: number, value: string): void {
    this.draftLines.update((lines) =>
      lines.map((line, i) => (i === index ? { ...line, reason: value } : line)),
    );
  }

  updateAmount(index: number, value: number | string | null): void {
    const amount =
      value === '' || value === null || value === undefined
        ? null
        : Number(value);
    this.draftLines.update((lines) =>
      lines.map((line, i) =>
        i === index
          ? {
              ...line,
              amount: Number.isFinite(amount as number) ? amount : null,
            }
          : line,
      ),
    );
  }

  save(): void {
    const current = this.quote();
    if (!current) return;

    const surcharges = this.draftLines()
      .map((line) => ({
        reason: line.reason.trim(),
        amount: Number(line.amount) || 0,
      }))
      .filter((line) => line.reason && line.amount !== 0);

    this.saving.set(true);
    this.error.set(null);
    this.quotesApi
      .upsert({
        projectId: current.projectId,
        cuttingPlanId: current.cuttingPlanId ?? null,
        notes: this.notes.trim() || null,
        surcharges,
      })
      .subscribe({
        next: (quote) => {
          this.applyQuote(quote);
          this.saving.set(false);
          this.notice.set('quote.saved');
        },
        error: (err) => {
          this.saving.set(false);
          this.error.set(apiErrorMessage(err, 'common.saveError'));
        },
      });
  }

  exportPdf(): void {
    // Print uses #quoteInvoice only via global print CSS on body.
    document.body.classList.add('printing-quote-invoice');
    const cleanup = () => {
      document.body.classList.remove('printing-quote-invoice');
      window.removeEventListener('afterprint', cleanup);
    };
    window.addEventListener('afterprint', cleanup);
    window.print();
    // Fallback if afterprint is skipped
    setTimeout(cleanup, 1000);
  }
}
