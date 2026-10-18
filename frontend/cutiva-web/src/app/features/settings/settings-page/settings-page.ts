import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { Sector, SectorsApi } from '../../../core/api/sectors.api';
import { WorkshopsApi } from '../../../core/api/workshops.api';
import { WorkshopContextService } from '../../../core/services/workshop-context.service';

@Component({
  selector: 'app-settings-page',
  imports: [FormsModule, TranslatePipe],
  templateUrl: './settings-page.html',
  styleUrl: './settings-page.scss',
})
export class SettingsPage implements OnInit {
  private readonly workshopsApi = inject(WorkshopsApi);
  private readonly sectorsApi = inject(SectorsApi);
  readonly workshopContext = inject(WorkshopContextService);

  readonly sectors = signal<Sector[]>([]);
  readonly error = signal<string | null>(null);
  readonly notice = signal<string | null>(null);
  readonly draftNames = signal<Record<string, string>>({});

  workshopName = '';
  sectorCode = '';
  sectorName = '';

  ngOnInit(): void {
    this.reloadSectors();
  }

  workshopDraft(id: string, fallback: string): string {
    return this.draftNames()[id] ?? fallback;
  }

  setWorkshopDraft(id: string, value: string): void {
    this.draftNames.update((map) => ({ ...map, [id]: value }));
  }

  reloadSectors(): void {
    this.sectorsApi.list().subscribe({
      next: (sectors) => this.sectors.set(sectors),
      error: () => this.error.set('common.loadError'),
    });
  }

  createWorkshop(): void {
    if (!this.workshopName.trim()) return;
    this.workshopsApi.create(this.workshopName.trim()).subscribe({
      next: (created) => {
        this.workshopName = '';
        this.notice.set('settings.workshopCreated');
        this.workshopContext.reload();
        setTimeout(() => this.workshopContext.selectWorkshop(created.id), 400);
      },
      error: () => this.error.set('common.saveError'),
    });
  }

  renameWorkshop(id: string, fallback: string): void {
    const trimmed = this.workshopDraft(id, fallback).trim();
    if (!trimmed) return;
    this.workshopsApi.update(id, trimmed).subscribe({
      next: () => {
        this.notice.set('settings.workshopUpdated');
        this.workshopContext.reload();
      },
      error: () => this.error.set('common.saveError'),
    });
  }

  deleteWorkshop(id: string): void {
    this.workshopsApi.delete(id).subscribe({
      next: () => {
        this.notice.set('settings.workshopDeleted');
        this.workshopContext.reload();
      },
      error: () => this.error.set('common.deleteError'),
    });
  }

  createSector(): void {
    if (!this.sectorCode.trim() || !this.sectorName.trim()) return;
    this.sectorsApi
      .create({
        code: this.sectorCode.trim(),
        name: this.sectorName.trim(),
      })
      .subscribe({
        next: () => {
          this.sectorCode = '';
          this.sectorName = '';
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
}
