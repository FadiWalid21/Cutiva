import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AppLanguage, LanguageService } from '../../core/services/language.service';
import { WorkshopContextService } from '../../core/services/workshop-context.service';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, TranslatePipe],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell {
  private readonly languageService = inject(LanguageService);
  readonly workshopContext = inject(WorkshopContextService);

  readonly currentLanguage = this.languageService.currentLanguage;

  setLanguage(lang: AppLanguage): void {
    this.languageService.setLanguage(lang);
  }

  onWorkshopChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.workshopContext.selectWorkshop(select.value);
  }
}
