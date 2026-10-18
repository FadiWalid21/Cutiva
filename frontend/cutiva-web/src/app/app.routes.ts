import { Routes } from '@angular/router';
import { Shell } from './layout/shell/shell';
import { Dashboard } from './features/dashboard/dashboard';
import { ProjectCreate } from './features/projects/project-create/project-create';
import { ProjectList } from './features/projects/project-list/project-list';
import { ProjectWorkspace } from './features/projects/project-workspace/project-workspace';
import { PlanGenerate } from './features/cutting-plans/plan-generate/plan-generate';
import { PlanDetail } from './features/cutting-plans/plan-detail/plan-detail';
import { MaterialsPage } from './features/materials/materials-page/materials-page';
import { SettingsPage } from './features/settings/settings-page/settings-page';

export const routes: Routes = [
  {
    path: '',
    component: Shell,
    children: [
      { path: '', component: Dashboard },
      { path: 'projects/new', component: ProjectCreate },
      { path: 'projects/:projectId', component: ProjectWorkspace },
      { path: 'projects', component: ProjectList },
      { path: 'materials', component: MaterialsPage },
      { path: 'cutting-plans/new', component: PlanGenerate },
      { path: 'cutting-plans/:id', component: PlanDetail },
      { path: 'cutting-plans', component: PlanGenerate },
      { path: 'settings', component: SettingsPage },
    ],
  },
  { path: '**', redirectTo: '' },
];
