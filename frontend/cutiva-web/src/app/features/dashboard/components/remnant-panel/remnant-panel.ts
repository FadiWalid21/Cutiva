import { DecimalPipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { ProjectedLeftoverItem } from '../../data/dashboard.models';

@Component({
  selector: 'app-remnant-panel',
  imports: [TranslatePipe, DecimalPipe, RouterLink],
  templateUrl: './remnant-panel.html',
  styleUrl: './remnant-panel.scss',
})
export class RemnantPanel {
  readonly leftovers = input.required<ProjectedLeftoverItem[]>();
}
