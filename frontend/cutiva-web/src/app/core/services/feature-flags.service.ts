import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class FeatureFlagsService {
  readonly resourcesV2 = environment.features?.resourcesV2 === true;
  readonly productTemplates = environment.features?.productTemplates === true;
}
