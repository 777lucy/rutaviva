import { Injectable, inject, signal } from '@angular/core';
import { VehicleApiService } from '../../core/api/vehicle-api.service';
import { VehicleDetail } from '../../core/api/models/vehicle.model';
import { problemTitle } from '../../core/problem-title';
import { RefreshService, reloadOnRefresh } from '../../core/refresh.service';

@Injectable()
export class VehicleStateService {
  private readonly vehicleApi = inject(VehicleApiService);
  private readonly refresh = inject(RefreshService);
  private vehicleId: string | null = null;

  readonly detail = signal<VehicleDetail | null>(null);
  readonly loadError = signal<string | null>(null);

  constructor() {
    reloadOnRefresh(this.refresh, () => this.reload());
  }

  load(vehicleId: string): void {
    this.vehicleId = vehicleId;
    this.vehicleApi.getVehicle(vehicleId).subscribe({
      next: (vehicle) => {
        this.detail.set(vehicle);
        this.loadError.set(null);
      },
      error: (error: unknown) => this.loadError.set(problemTitle(error))
    });
  }

  private reload(): void {
    if (this.vehicleId) {
      this.load(this.vehicleId);
    }
  }
}
