import { Routes } from '@angular/router';
import { Shell } from './layout/shell/shell';
import { Home } from './features/home/home';

export const routes: Routes = [
  {
    path: '',
    component: Shell,
    children: [
      { path: '', component: Home },
      {
        path: 'workshops',
        loadComponent: () =>
          import('./features/home/home').then((m) => m.Home),
        data: { placeholder: 'workshops' },
      },
      {
        path: 'materials',
        loadComponent: () =>
          import('./features/home/home').then((m) => m.Home),
        data: { placeholder: 'materials' },
      },
      {
        path: 'cutting-plans',
        loadComponent: () =>
          import('./features/home/home').then((m) => m.Home),
        data: { placeholder: 'cutting-plans' },
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
