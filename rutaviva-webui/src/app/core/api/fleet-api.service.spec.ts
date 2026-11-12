import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { FleetVehicle } from './models/fleet.model';
import { FleetApiService } from './fleet-api.service';

describe('FleetApiService (Contract)', () => {
  let service: FleetApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(FleetApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('GETs the current client fleet from /api/fleet and returns the typed list', () => {
    const payload: FleetVehicle[] = [
      { id: 'vehicle-1', plate: 'ABC-123', description: 'Camión', odometer: 58000, awaitingQuote: false, routines: [] }
    ];
    let fleet: FleetVehicle[] | undefined;

    service.getFleet().subscribe((result) => (fleet = result));
    const request = http.expectOne(`${environment.apiUrl}/api/fleet`);
    expect(request.request.method).toBe('GET');
    request.flush(payload);

    expect(fleet).toEqual(payload);
  });
});
