import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiUrl } from './api-url';
import { ReadingOutcome } from './models/telemetry.model';

@Injectable({ providedIn: 'root' })
export class TelemetryApiService {
  private readonly http = inject(HttpClient);

  registerReading(vehicleId: string, kilometers: number): Observable<ReadingOutcome> {
    return this.http.post<ReadingOutcome>(apiUrl('/api/telemetry/readings'), { vehicleId, kilometers });
  }
}
