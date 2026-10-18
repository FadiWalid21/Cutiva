import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'app-quick-create-cta',
  imports: [TranslatePipe, RouterLink],
  templateUrl: './quick-create-cta.html',
  styleUrl: './quick-create-cta.scss',
})
export class QuickCreateCta {}
