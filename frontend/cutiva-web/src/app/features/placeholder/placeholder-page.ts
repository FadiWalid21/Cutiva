import { Component } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'app-placeholder-page',
  imports: [TranslatePipe],
  template: `
    <section class="card placeholder">
      <p>{{ 'placeholder.comingSoon' | translate }}</p>
    </section>
  `,
  styles: `
    .placeholder {
      padding: 2rem;
      color: var(--color-muted);
    }
  `,
})
export class PlaceholderPage {}
