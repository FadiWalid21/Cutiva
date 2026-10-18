import { Component, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { isI18nKey } from '../../core/http/api-error';

@Component({
  selector: 'app-form-field-error',
  imports: [TranslatePipe],
  template: `
    @if (message()) {
      <p class="field-error" role="alert">
        @if (isI18nKey(message()!)) {
          {{ message()! | translate }}
        } @else {
          {{ message()! }}
        }
      </p>
    }
  `,
  styles: `
    .field-error {
      margin: 0.35rem 0 0;
      font-size: 0.8rem;
      color: var(--color-danger);
      line-height: 1.35;
    }
  `,
})
export class FormFieldError {
  readonly message = input<string | null>(null);
  readonly isI18nKey = isI18nKey;
}
