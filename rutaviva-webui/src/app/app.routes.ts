import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/fleet/fleet-page.component').then((module) => module.FleetPageComponent)
  },
  {
    path: 'vehicles/:id',
    loadComponent: () => import('./features/vehicle/vehicle-page.component').then((module) => module.VehiclePageComponent)
  },
  {
    path: 'orders/:id',
    loadComponent: () => import('./features/order/order-page.component').then((module) => module.OrderPageComponent)
  }
];
