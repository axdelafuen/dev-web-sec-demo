import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { 
    path: 'home', 
    loadComponent: () => import('./home/home.component').then(m => m.HomeComponent) 
  },
  { 
    path: 'vulnerable-login', 
    loadComponent: () => import('./vulnerable-login/vulnerable-login.component').then(m => m.VulnerableLoginComponent) 
  },
  { 
    path: 'secure-login', 
    loadComponent: () => import('./secure-login/secure-login.component').then(m => m.SecureLoginComponent) 
  },
  { 
    path: 'connected', 
    loadComponent: () => import('./connected/connected.component').then(m => m.ConnectedComponent) 
  }
];
