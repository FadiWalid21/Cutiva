import { Routes } from '@angular/router';
import { adminGuard, authGuard, guestGuard } from './core/guards/auth.guard';
import {
  catalogManageActivate,
  planFeatureActivate,
} from './core/guards/feature.guard';
import { PlanFeatureCodes } from './core/constants/plan-features';
import { Shell } from './layout/shell/shell';
import { AdminShell } from './layout/admin-shell/admin-shell';
import { Dashboard } from './features/dashboard/dashboard';
import { ProjectCreate } from './features/projects/project-create/project-create';
import { ProjectList } from './features/projects/project-list/project-list';
import { ProjectWorkspace } from './features/projects/project-workspace/project-workspace';
import { PlanGenerate } from './features/cutting-plans/plan-generate/plan-generate';
import { PlanDetail } from './features/cutting-plans/plan-detail/plan-detail';
import { ResourcesPage } from './features/resources/resources-page/resources-page';
import { TemplatesPage } from './features/product-templates/templates-page/templates-page';
import { SettingsPage } from './features/settings/settings-page/settings-page';
import { QuotePage } from './features/quotes/quote-page/quote-page';
import { QuotesList } from './features/quotes/quotes-list/quotes-list';
import { ReportsPage } from './features/reports/reports-page/reports-page';
import { GuidePage } from './features/guide/guide-page/guide-page';
import { Login } from './features/auth/login/login';
import { ForgotPassword } from './features/auth/forgot-password/forgot-password';
import { LandingPage } from './features/landing/landing-page';
import { AdminOverview } from './features/admin/admin-overview/admin-overview';
import { AdminClients } from './features/admin/admin-clients/admin-clients';
import { AdminPlans } from './features/admin/admin-plans/admin-plans';
import { AdminFeedback } from './features/admin/admin-feedback/admin-feedback';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    component: LandingPage,
  },
  {
    path: 'login',
    canActivate: [guestGuard],
    component: Login,
  },
  {
    path: 'forgot-password',
    canActivate: [guestGuard],
    component: ForgotPassword,
  },
  {
    path: 'signup',
    redirectTo: 'login',
  },
  {
    path: 'admin',
    component: AdminShell,
    canActivate: [adminGuard],
    children: [
      { path: '', pathMatch: 'full', component: AdminOverview },
      { path: 'clients', component: AdminClients },
      { path: 'plans', component: AdminPlans },
      { path: 'feedback', component: AdminFeedback },
    ],
  },
  {
    path: '',
    component: Shell,
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', component: Dashboard },
      { path: 'projects/new', component: ProjectCreate },
      { path: 'projects/:projectId', component: ProjectWorkspace },
      { path: 'projects', component: ProjectList },
      { path: 'materials', redirectTo: 'resources', pathMatch: 'full' },
      {
        path: 'resources',
        canActivate: [
          catalogManageActivate,
          planFeatureActivate(PlanFeatureCodes.resources),
        ],
        component: ResourcesPage,
      },
      {
        path: 'product-templates',
        canActivate: [
          catalogManageActivate,
          planFeatureActivate(PlanFeatureCodes.productTemplates),
        ],
        component: TemplatesPage,
      },
      { path: 'cutting-plans/new', component: PlanGenerate },
      { path: 'cutting-plans/:id', component: PlanDetail },
      { path: 'cutting-plans', component: PlanGenerate },
      {
        path: 'quotes',
        canActivate: [planFeatureActivate(PlanFeatureCodes.clientQuotes)],
        component: QuotesList,
      },
      {
        path: 'quotes/:quoteId',
        canActivate: [planFeatureActivate(PlanFeatureCodes.clientQuotes)],
        component: QuotePage,
      },
      {
        path: 'reports',
        canActivate: [planFeatureActivate(PlanFeatureCodes.workshopReports)],
        component: ReportsPage,
      },
      { path: 'guide', component: GuidePage },
      { path: 'settings', component: SettingsPage },
    ],
  },
  { path: '**', redirectTo: '' },
];
