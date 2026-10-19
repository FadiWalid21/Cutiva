import { Component, inject } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { PwaInstallService } from '../../core/services/pwa-install.service';
import { PwaUpdateService } from '../../core/services/pwa-update.service';

@Component({
  selector: 'app-pwa-install-banner',
  imports: [TranslatePipe],
  templateUrl: './pwa-install-banner.html',
  styleUrl: './pwa-install-banner.scss',
})
export class PwaInstallBanner {
  readonly pwa = inject(PwaInstallService);
  readonly updates = inject(PwaUpdateService);

  install(): void {
    void this.pwa.promptInstall();
  }

  hide(): void {
    this.pwa.dismissBanner();
  }

  closeHelp(): void {
    this.pwa.closeHelp();
  }

  applyUpdate(): void {
    void this.updates.activateUpdate();
  }

  deferUpdate(): void {
    this.updates.dismissForSession();
  }
}
