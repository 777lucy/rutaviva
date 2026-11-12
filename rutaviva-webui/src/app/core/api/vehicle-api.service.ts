import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiUrl } from './api-url';
import { VehicleDetail } from './models/vehicle.model';

@Injectable({ providedIn: 'root' })
export class VehicleApiService {
  private readonly http = inject(HttpClient);

  getVehicle(vehicleId: string): Observable<VehicleDetail> {
    return this.http.get<VehicleDetail>(apiUrl(`/api/vehicles/${vehicleId}`));
  }
}
