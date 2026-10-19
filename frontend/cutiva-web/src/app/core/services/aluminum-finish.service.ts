import { Injectable, computed, signal } from '@angular/core';

export type AluminumFinishId =
  | 'brown'
  | 'silver'
  | 'grey'
  | 'black'
  | 'white'
  | 'bronze';

export interface AluminumFinish {
  id: AluminumFinishId;
  labelKey: string;
  /** Mid metal tone */
  mid: string;
  light: string;
  dark: string;
}

const STORAGE_KEY = 'cutiva.aluminumFinish';

export const ALUMINUM_FINISHES: AluminumFinish[] = [
  {
    id: 'brown',
    labelKey: 'preview.finish.brown',
    mid: '#6b4423',
    light: '#8b5a2b',
    dark: '#3e2612',
  },
  {
    id: 'silver',
    labelKey: 'preview.finish.silver',
    mid: '#b0b6bc',
    light: '#e2e6ea',
    dark: '#7a8188',
  },
  {
    id: 'grey',
    labelKey: 'preview.finish.grey',
    mid: '#6e747a',
    light: '#9aa0a6',
    dark: '#3f4449',
  },
  {
    id: 'black',
    labelKey: 'preview.finish.black',
    mid: '#2a2d30',
    light: '#4a4f55',
    dark: '#121416',
  },
  {
    id: 'white',
    labelKey: 'preview.finish.white',
    mid: '#e8eaed',
    light: '#ffffff',
    dark: '#b8bcc2',
  },
  {
    id: 'bronze',
    labelKey: 'preview.finish.bronze',
    mid: '#8a6a3d',
    light: '#b8924f',
    dark: '#5a4324',
  },
];

@Injectable({ providedIn: 'root' })
export class AluminumFinishService {
  private readonly finishIdSignal = signal<AluminumFinishId>(this.readStored());

  readonly finishId = this.finishIdSignal.asReadonly();
  readonly finishes = ALUMINUM_FINISHES;

  readonly finish = computed(
    () =>
      ALUMINUM_FINISHES.find((f) => f.id === this.finishIdSignal()) ??
      ALUMINUM_FINISHES[1],
  );

  setFinish(id: AluminumFinishId): void {
    this.finishIdSignal.set(id);
    localStorage.setItem(STORAGE_KEY, id);
  }

  private readStored(): AluminumFinishId {
    const value = localStorage.getItem(STORAGE_KEY);
    if (value && ALUMINUM_FINISHES.some((f) => f.id === value)) {
      return value as AluminumFinishId;
    }
    return 'silver';
  }
}
