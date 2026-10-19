import { Component, inject } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import {
  AluminumFinishId,
  AluminumFinishService,
} from '../../core/services/aluminum-finish.service';
import {
  GlassTintId,
  GlassTintService,
} from '../../core/services/glass-tint.service';

@Component({
  selector: 'app-aluminum-finish-bar',
  imports: [TranslatePipe],
  templateUrl: './aluminum-finish-bar.html',
  styleUrl: './aluminum-finish-bar.scss',
})
export class AluminumFinishBar {
  readonly finishService = inject(AluminumFinishService);
  readonly glassService = inject(GlassTintService);

  selectFinish(id: AluminumFinishId): void {
    this.finishService.setFinish(id);
  }

  selectGlass(id: GlassTintId): void {
    this.glassService.setTint(id);
  }
}
