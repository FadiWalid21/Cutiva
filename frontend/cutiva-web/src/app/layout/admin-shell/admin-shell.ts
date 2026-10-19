import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthService } from '../../core/services/auth.service';
import { AppLanguage, LanguageService } from '../../core/services/language.service';
import { PwaInstallService } from '../../core/services/pwa-install.service';

@Component({
  selector: 'app-admin-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, TranslatePipe],
  templateUrl: './admin-shell.html',
  styleUrl: './admin-shell.scss',
})
export class AdminShell {
  private readonly languageService = inject(LanguageService);
  private readonly auth = inject(AuthService);
  readonly pwa = inject(PwaInstallService);

  readonly currentUser = this.auth.currentUser;
  readonly currentLanguage = this.languageService.currentLanguage;
  readonly showInstall = this.pwa.showInstallControl;

  setLanguage(lang: AppLanguage): void {
    void this.languageService.setLanguage(lang);
  }

  logout(): void {
    this.auth.logout();
  }

  installApp(): void {
    void this.pwa.promptInstall();
  }
}
