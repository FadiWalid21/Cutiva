import { Injectable, computed, signal } from '@angular/core';

export type GlassTintId =
  | 'clear'
  | 'blue'
  | 'green'
  | 'smoke'
  | 'bronze'
  | 'dark';

export interface GlassTint {
  id: GlassTintId;
  labelKey: string;
  top: string;
  mid: string;
  bottom: string;
  topOpacity: number;
  midOpacity: number;
  bottomOpacity: number;
}

const STORAGE_KEY = 'cutiva.glassTint';

export const GLASS_TINTS: GlassTint[] = [
  {
    id: 'clear',
    labelKey: 'preview.glass.clear',
    top: '#d9f2ff',
    mid: '#8ec8e8',
    bottom: '#5a9fc4',
    topOpacity: 0.85,
    midOpacity: 0.55,
    bottomOpacity: 0.4,
  },
  {
    id: 'blue',
    labelKey: 'preview.glass.blue',
    top: '#b8d9ff',
    mid: '#4a8fd4',
    bottom: '#1f5f9e',
    topOpacity: 0.88,
    midOpacity: 0.62,
    bottomOpacity: 0.48,
  },
  {
    id: 'green',
    labelKey: 'preview.glass.green',
    top: '#c8efd8',
    mid: '#5aaa7a',
    bottom: '#2f6b4a',
    topOpacity: 0.86,
    midOpacity: 0.58,
    bottomOpacity: 0.45,
  },
  {
    id: 'smoke',
    labelKey: 'preview.glass.smoke',
    top: '#d5d8dc',
    mid: '#8b9199',
    bottom: '#4e545c',
    topOpacity: 0.82,
    midOpacity: 0.6,
    bottomOpacity: 0.5,
  },
  {
    id: 'bronze',
    labelKey: 'preview.glass.bronze',
    top: '#f0d9b0',
    mid: '#c4924a',
    bottom: '#7a5424',
    topOpacity: 0.84,
    midOpacity: 0.58,
    bottomOpacity: 0.46,
  },
  {
    id: 'dark',
    labelKey: 'preview.glass.dark',
    top: '#6a727c',
    mid: '#3a4149',
    bottom: '#1a1e24',
    topOpacity: 0.78,
    midOpacity: 0.7,
    bottomOpacity: 0.65,
  },
];

@Injectable({ providedIn: 'root' })
export class GlassTintService {
  private readonly tintIdSignal = signal<GlassTintId>(this.readStored());

  readonly tintId = this.tintIdSignal.asReadonly();
  readonly tints = GLASS_TINTS;

  readonly tint = computed(
    () => GLASS_TINTS.find((t) => t.id === this.tintIdSignal()) ?? GLASS_TINTS[0],
  );

  setTint(id: GlassTintId): void {
    this.tintIdSignal.set(id);
    localStorage.setItem(STORAGE_KEY, id);
  }

  private readStored(): GlassTintId {
    const value = localStorage.getItem(STORAGE_KEY);
    if (value && GLASS_TINTS.some((t) => t.id === value)) {
      return value as GlassTintId;
    }
    return 'clear';
  }
}
