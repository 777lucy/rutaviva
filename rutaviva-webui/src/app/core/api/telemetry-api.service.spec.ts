import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { TelemetryApiService } from './telemetry-api.service';

describe('TelemetryApiService (Contract)', () => {
  let service: TelemetryApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(TelemetryApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('POSTs the vehicle and kilometers to /api/telemetry/readings and returns the outcome', () => {
    let outcome: string | undefined;

    service.registerReading('vehicle-1', 59000).subscribe((result) => (outcome = result.result));
    const request = http.expectOne(`${environment.apiUrl}/api/telemetry/readings`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ vehicleId: 'vehicle-1', kilometers: 59000 });
    request.flush({ result: 'Ignored' });

    expect(outcome).toBe('Ignored');
  });
});
