import { Component, inject } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { PwaInstallService } from '../../core/services/pwa-install.service';

@Component({
  selector: 'app-pwa-install-banner',
  imports: [TranslatePipe],
  templateUrl: './pwa-install-banner.html',
  styleUrl: './pwa-install-banner.scss',
})
export class PwaInstallBanner {
  readonly pwa = inject(PwaInstallService);

  install(): void {
    void this.pwa.promptInstall();
  }

  hide(): void {
    this.pwa.dismissBanner();
  }

  closeHelp(): void {
    this.pwa.closeHelp();
  }
}
