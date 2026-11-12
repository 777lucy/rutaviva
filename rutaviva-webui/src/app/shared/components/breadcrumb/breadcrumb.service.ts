import { Injectable, computed, signal } from '@angular/core';

export interface Crumb {
  label: string;
  link: string[] | null;
}

const FleetCrumbLabel = 'Flota';

@Injectable({ providedIn: 'root' })
export class BreadcrumbService {
  private readonly trail = signal<Crumb[]>([]);

  readonly crumbs = computed<Crumb[]>(() => {
    const trail = this.trail();
    return [{ label: FleetCrumbLabel, link: trail.length > 0 ? ['/'] : null }, ...trail];
  });

  show(trail: Crumb[]): void {
    this.trail.set(trail);
  }
}
