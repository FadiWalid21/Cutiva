import { Component, OnInit, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { forkJoin } from 'rxjs';
import { AuthApi } from '../../../core/api/auth.api';
import { Material, MaterialsApi } from '../../../core/api/materials.api';
import { Sector, SectorsApi } from '../../../core/api/sectors.api';
import { WorkshopsApi } from '../../../core/api/workshops.api';
import { MARKET_MATERIALS_SECTOR_CODE } from '../../../core/constants/market';
import { apiErrorMessage } from '../../../core/http/api-error';
import { WorkshopContextService } from '../../../core/services/workshop-context.service';

type PasswordStep = 'current' | 'new';

@Component({
  selector: 'app-settings-page',
  imports: [FormsModule, TranslatePipe, RouterLink],
  templateUrl: './settings-page.html',
  styleUrl: './settings-page.scss',
})
export class SettingsPage implements OnInit {
  private readonly workshopsApi = inject(WorkshopsApi);
  private readonly sectorsApi = inject(SectorsApi);
  private readonly materialsApi = inject(MaterialsApi);
  private readonly authApi = inject(AuthApi);
  readonly workshopContext = inject(WorkshopContextService);
  readonly marketLink = '/resources';

  readonly sectors = signal<Sector[]>([]);
  readonly profileMaterials = signal<Material[]>([]);
  readonly materialsReady = signal(false);
  readonly error = signal<string | null>(null);
  readonly notice = signal<string | null>(null);
  readonly linkDrafts = signal<Record<string, string>>({});

  readonly passwordStep = signal<PasswordStep>('current');
  readonly passwordSaving = signal(false);
  readonly passwordError = signal<string | null>(null);
  currentPassword = '';
  newPassword = '';
  confirmPassword = '';

  readonly primaryWorkshop = computed(
    () =>
      this.workshopContext.selectedWorkshop() ??
      this.workshopContext.workshops()[0] ??
      null,
  );

  workshopNameDraft = '';
  productionPricePerM2: number | null = null;
  sectorCode = '';
  sectorName = '';
  sectorMaterialId = '';

  constructor() {
    effect(() => {
      const workshop = this.primaryWorkshop();
      if (workshop) {
        this.workshopNameDraft = workshop.name;
        this.productionPricePerM2 = workshop.productionPricePerM2 ?? null;
      }
    });
  }

  ngOnInit(): void {
    this.reloadAll();
  }

  reloadAll(): void {
    this.materialsReady.set(false);
    forkJoin({
      materials: this.materialsApi.list(),
      sectors: this.sectorsApi.list(),
    }).subscribe({
      next: ({ materials, sectors }) => {
        this.profileMaterials.set(materials);
        this.materialsReady.set(true);
        if (!this.sectorMaterialId && materials.length) {
          this.sectorMaterialId = materials[0].id;
        }
        this.sectors.set(
          sectors.filter(
            (s) => s.code.toUpperCase() !== MARKET_MATERIALS_SECTOR_CODE,
          ),
        );
        this.linkDrafts.set({});
      },
      error: () => this.error.set('common.loadError'),
    });
  }

  reloadSectors(): void {
    this.sectorsApi.list().subscribe({
      next: (sectors) => {
        this.sectors.set(
          sectors.filter(
            (s) => s.code.toUpperCase() !== MARKET_MATERIALS_SECTOR_CODE,
          ),
        );
        this.linkDrafts.set({});
      },
      error: () => this.error.set('common.loadError'),
    });
  }

  renamePrimaryWorkshop(): void {
    const workshop = this.primaryWorkshop();
    const trimmed = this.workshopNameDraft.trim();
    if (!workshop || !trimmed) return;

    const rate = this.productionPricePerM2;
    const productionPricePerM2 =
      rate != null && Number(rate) > 0 ? Number(rate) : null;

    this.workshopsApi
      .update(workshop.id, trimmed, productionPricePerM2)
      .subscribe({
        next: () => {
          this.notice.set('settings.workshopUpdated');
          this.workshopContext.reload();
        },
        error: () => this.error.set('common.saveError'),
      });
  }

  createSector(): void {
    if (!this.sectorCode.trim() || !this.sectorName.trim()) return;

    if (this.sectorCode.trim().toUpperCase() === MARKET_MATERIALS_SECTOR_CODE) {
      this.error.set('settings.reservedSectorCode');
      return;
    }

    if (this.profileMaterials().length === 0) {
      this.error.set('settings.noMarketMaterials');
      return;
    }

    if (!this.sectorMaterialId) {
      this.error.set('settings.materialRequired');
      return;
    }

    this.error.set(null);
    this.sectorsApi
      .create({
        code: this.sectorCode.trim(),
        name: this.sectorName.trim(),
        materialId: this.sectorMaterialId,
      })
      .subscribe({
        next: () => {
          this.sectorCode = '';
          this.sectorName = '';
          this.notice.set('settings.sectorCreated');
          this.reloadSectors();
        },
        error: () => this.error.set('common.saveError'),
      });
  }

  saveSectorMaterial(sector: Sector): void {
    const materialId = this.linkDraft(sector);
    if (!materialId) {
      this.error.set('settings.materialRequired');
      return;
    }

    this.error.set(null);
    this.sectorsApi
      .update(sector.id, {
        code: sector.code,
        name: sector.name,
        materialId,
      })
      .subscribe({
        next: () => {
          this.notice.set('settings.sectorLinked');
          this.reloadSectors();
        },
        error: () => this.error.set('common.saveError'),
      });
  }

  deleteSector(id: string): void {
    this.sectorsApi.delete(id).subscribe({
      next: () => this.reloadSectors(),
      error: () => this.error.set('common.deleteError'),
    });
  }

  linkDraft(sector: Sector): string {
    return this.linkDrafts()[sector.id] ?? sector.materialId ?? '';
  }

  setLinkDraft(id: string, value: string): void {
    this.linkDrafts.update((map) => ({ ...map, [id]: value }));
  }

  materialLabel(sector: Sector): string {
    if (sector.materialCode && sector.materialName) {
      return `${sector.materialCode} — ${sector.materialName}`;
    }
    return '';
  }

  continuePasswordStep(): void {
    this.passwordError.set(null);
    if (!this.currentPassword.trim()) {
      this.passwordError.set('settings.passwordCurrentRequired');
      return;
    }
    this.passwordStep.set('new');
  }

  backPasswordStep(): void {
    this.passwordError.set(null);
    this.newPassword = '';
    this.confirmPassword = '';
    this.passwordStep.set('current');
  }

  submitPasswordChange(): void {
    this.passwordError.set(null);
    const current = this.currentPassword;
    const next = this.newPassword;
    const confirm = this.confirmPassword;

    if (!next || next.length < 8) {
      this.passwordError.set('auth.validation.passwordMin');
      return;
    }
    if (next !== confirm) {
      this.passwordError.set('settings.passwordMismatch');
      return;
    }
    if (current === next) {
      this.passwordError.set('auth.change.sameAsCurrent');
      return;
    }

    this.passwordSaving.set(true);
    this.authApi.changePassword({ currentPassword: current, newPassword: next }).subscribe({
      next: () => {
        this.passwordSaving.set(false);
        this.resetPasswordForm();
        this.notice.set('settings.passwordChanged');
      },
      error: (err) => {
        this.passwordSaving.set(false);
        const key = apiErrorMessage(err, 'settings.passwordChangeError');
        this.passwordError.set(key);
        if (key === 'auth.change.wrongCurrent') {
          this.passwordStep.set('current');
          this.newPassword = '';
          this.confirmPassword = '';
        }
      },
    });
  }

  private resetPasswordForm(): void {
    this.currentPassword = '';
    this.newPassword = '';
    this.confirmPassword = '';
    this.passwordStep.set('current');
    this.passwordError.set(null);
  }
}
