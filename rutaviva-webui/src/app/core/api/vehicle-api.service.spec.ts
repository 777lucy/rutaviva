import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { VehicleApiService } from './vehicle-api.service';
import { VehicleDetail } from './models/vehicle.model';

describe('VehicleApiService (Contract)', () => {
  let service: VehicleApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(VehicleApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('GETs one vehicle from /api/vehicles/{id} and returns its routines with open orders', () => {
    const payload: VehicleDetail = {
      id: 'vehicle-1',
      plate: 'ABC-123',
      description: 'Camión',
      odometer: 59000,
      workshopName: 'Taller Central',
      routines: [
        {
          id: 'routine-1',
          name: 'Aceite',
          interval: 'cada 10.000 km o 6 meses',
          status: 'DueSoon',
          openOrder: { id: 'order-1', status: 'Created' },
          baseline: { kilometers: 50000, date: '2026-01-01' },
          remaining: { kilometers: 1000, days: 20 }
        }
      ]
    };
    let vehicle: VehicleDetail | undefined;

    service.getVehicle('vehicle-1').subscribe((result) => (vehicle = result));
    const request = http.expectOne(`${environment.apiUrl}/api/vehicles/vehicle-1`);
    expect(request.request.method).toBe('GET');
    request.flush(payload);

    expect(vehicle).toEqual(payload);
  });
});
