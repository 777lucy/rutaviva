import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiUrl } from './api-url';
import { FleetVehicle } from './models/fleet.model';

@Injectable({ providedIn: 'root' })
export class FleetApiService {
  private readonly http = inject(HttpClient);

  getFleet(): Observable<FleetVehicle[]> {
    return this.http.get<FleetVehicle[]>(apiUrl('/api/fleet'));
  }
}
