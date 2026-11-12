import { Injectable, inject, signal } from '@angular/core';
import { Observable, defaultIfEmpty, forkJoin, map, switchMap } from 'rxjs';
import { FleetApiService } from '../../core/api/fleet-api.service';
import { FleetVehicle } from '../../core/api/models/fleet.model';
import { RecordedEvent } from '../../core/api/models/recorded-event.model';
import { SystemApiService } from '../../core/api/system-api.service';
import { VehicleApiService } from '../../core/api/vehicle-api.service';
import { problemTitle } from '../../core/problem-title';
import { RefreshService, reloadOnRefresh } from '../../core/refresh.service';
import { OpenOrderItem, openOrdersOf } from '../../shared/open-orders';
import { DueSoonBanner, activeDueSoonBanners } from './due-soon-banner';

interface LoadedFleet {
  fleet: FleetVehicle[];
  events: RecordedEvent[];
  openOrders: OpenOrderItem[];
}

@Injectable({ providedIn: 'root' })
export class FleetStateService {
  private readonly fleetApi = inject(FleetApiService);
  private readonly systemApi = inject(SystemApiService);
  private readonly vehicleApi = inject(VehicleApiService);
  private readonly refresh = inject(RefreshService);

  readonly vehicles = signal<FleetVehicle[]>([]);
  readonly openOrders = signal<OpenOrderItem[]>([]);
  readonly banners = signal<DueSoonBanner[]>([]);
  readonly loaded = signal(false);
  readonly loadError = signal<string | null>(null);

  constructor() {
    reloadOnRefresh(this.refresh, () => this.load());
  }

  load(): void {
    forkJoin({ fleet: this.fleetApi.getFleet(), events: this.systemApi.getEvents() })
      .pipe(switchMap(({ fleet, events }) => this.withOpenOrders(fleet, events)))
      .subscribe({
        next: (loaded) => this.applyLoaded(loaded),
        error: (error: unknown) => this.loadError.set(problemTitle(error))
      });
  }

  private withOpenOrders(fleet: FleetVehicle[], events: RecordedEvent[]): Observable<LoadedFleet> {
    return forkJoin(fleet.map((vehicle) => this.vehicleApi.getVehicle(vehicle.id))).pipe(
      defaultIfEmpty([]),
      map((details) => ({ fleet, events, openOrders: openOrdersOf(details) }))
    );
  }

  private applyLoaded({ fleet, events, openOrders }: LoadedFleet): void {
    this.vehicles.set(fleet);
    this.openOrders.set(openOrders);
    this.banners.set(activeDueSoonBanners(events, fleet));
    this.loaded.set(true);
    this.loadError.set(null);
  }
}
