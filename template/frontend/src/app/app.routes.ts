import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/login/login').then(m => m.LoginComponent) },
  {
    path: 'sales',
    canActivate: [authGuard],
    children: [
      { path: '', loadComponent: () => import('./features/sales/sales-list/sales-list').then(m => m.SalesListComponent) },
      { path: 'new', loadComponent: () => import('./features/sales/sales-form/sales-form').then(m => m.SalesFormComponent) },
      { path: ':id', loadComponent: () => import('./features/sales/sales-detail/sales-detail').then(m => m.SalesDetailComponent) },
      { path: ':id/edit', loadComponent: () => import('./features/sales/sales-form/sales-form').then(m => m.SalesFormComponent) },
    ]
  },
  { path: '', redirectTo: 'sales', pathMatch: 'full' },
  { path: '**', redirectTo: 'sales' }
];
