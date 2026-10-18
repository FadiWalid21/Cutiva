import {
  AfterViewInit,
  Component,
  ElementRef,
  inject,
  viewChild,
} from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { MotionService } from '../../core/motion/motion.service';
import { AuthService } from '../../core/services/auth.service';
import { AppLanguage, LanguageService } from '../../core/services/language.service';
import { WorkshopContextService } from '../../core/services/workshop-context.service';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, TranslatePipe],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell implements AfterViewInit {
  private readonly languageService = inject(LanguageService);
  private readonly auth = inject(AuthService);
  private readonly motion = inject(MotionService);
  readonly workshopContext = inject(WorkshopContextService);

  private readonly shellRoot = viewChild<ElementRef<HTMLElement>>('shellRoot');
  private readonly navLinks = viewChild<ElementRef<HTMLElement>>('navLinks');

  readonly currentLanguage = this.languageService.currentLanguage;
  readonly currentUser = this.auth.currentUser;

  ngAfterViewInit(): void {
    this.motion.pageEnter(this.shellRoot()?.nativeElement ?? null);
    const nav = this.navLinks()?.nativeElement;
    if (nav) {
      this.motion.staggerIn(nav.querySelectorAll('a'));
    }
  }

  setLanguage(lang: AppLanguage): void {
    this.languageService.setLanguage(lang);
  }

  onWorkshopChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.workshopContext.selectWorkshop(select.value);
  }

  logout(): void {
    this.auth.logout();
  }
}
