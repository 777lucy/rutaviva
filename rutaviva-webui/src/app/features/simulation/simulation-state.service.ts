import { Injectable, inject, signal } from '@angular/core';
import { defaultIfEmpty, finalize, forkJoin, Observable, switchMap, tap } from 'rxjs';
import { FleetApiService } from '../../core/api/fleet-api.service';
import { VehicleApiService } from '../../core/api/vehicle-api.service';
import { WorkshopApiService } from '../../core/api/workshop-api.service';
import { FleetVehicle } from '../../core/api/models/fleet.model';
import { VehicleDetail } from '../../core/api/models/vehicle.model';
import { problemTitle } from '../../core/problem-title';
import { RefreshService, reloadOnRefresh } from '../../core/refresh.service';
import { OpenOrderItem, openOrdersOf } from '../../shared/open-orders';

@Injectable()
export class SimulationStateService {
  private readonly fleetApi = inject(FleetApiService);
  private readonly vehicleApi = inject(VehicleApiService);
  private readonly workshopApi = inject(WorkshopApiService);
  private readonly refresh = inject(RefreshService);

  readonly vehicles = signal<FleetVehicle[]>([]);
  readonly details = signal<VehicleDetail[]>([]);
  readonly openOrders = signal<OpenOrderItem[]>([]);
  readonly loadError = signal<string | null>(null);
  readonly workshopBusy = signal(false);
  readonly workshopError = signal<string | null>(null);

  constructor() {
    reloadOnRefresh(this.refresh, () => this.load());
  }

  load(): void {
    this.fleetApi
      .getFleet()
      .pipe(
        tap((vehicles) => this.vehicles.set(vehicles)),
        switchMap((vehicles) => this.loadDetails(vehicles))
      )
      .subscribe({
        next: (details) => {
          this.details.set(details);
          this.openOrders.set(openOrdersOf(details));
          this.loadError.set(null);
        },
        error: (error: unknown) => this.loadError.set(problemTitle(error))
      });
  }

  quote(orderId: string, amount: number): void {
    this.runWorkshopCommand(this.workshopApi.submitQuote(orderId, amount));
  }

  start(orderId: string): void {
    this.runWorkshopCommand(this.workshopApi.startService(orderId));
  }

  complete(orderId: string, kilometers: number, performedOn: string): void {
    this.runWorkshopCommand(this.workshopApi.completeService(orderId, kilometers, performedOn));
  }

  private loadDetails(vehicles: FleetVehicle[]): Observable<VehicleDetail[]> {
    return forkJoin(vehicles.map((vehicle) => this.vehicleApi.getVehicle(vehicle.id))).pipe(defaultIfEmpty([]));
  }

  private runWorkshopCommand(command: Observable<void>): void {
    this.workshopBusy.set(true);
    this.workshopError.set(null);
    command.pipe(finalize(() => this.workshopBusy.set(false))).subscribe({
      next: () => this.refresh.notifyCommandSucceeded(),
      error: (error: unknown) => this.workshopError.set(problemTitle(error))
    });
  }
}
