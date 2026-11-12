import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { FleetApiService } from '../../core/api/fleet-api.service';
import { FleetVehicle } from '../../core/api/models/fleet.model';
import { VehicleDetail } from '../../core/api/models/vehicle.model';
import { SystemApiService } from '../../core/api/system-api.service';
import { VehicleApiService } from '../../core/api/vehicle-api.service';
import { RefreshService } from '../../core/refresh.service';
import { FleetStateService } from './fleet-state.service';

const fleet: FleetVehicle[] = [
  { id: 'vehicle-1', plate: 'ABC-123', description: 'Camión', odometer: 58000, awaitingQuote: false, routines: [] }
];

const detail: VehicleDetail = {
  id: 'vehicle-1',
  plate: 'ABC-123',
  description: 'Camión',
  odometer: 58000,
  workshopName: 'Taller Central',
  routines: [
    {
      id: 'routine-oil',
      name: 'Aceite',
      interval: 'Cada 6 meses',
      status: 'Overdue',
      openOrder: { id: 'order-1', status: 'Created' },
      baseline: { kilometers: 58000, date: '2026-01-01' },
      remaining: { kilometers: null, days: -10 }
    }
  ]
};

function configure(fleetApi: unknown, systemApi: unknown = { getEvents: () => of([]) }): void {
  TestBed.configureTestingModule({
    providers: [
      { provide: FleetApiService, useValue: fleetApi },
      { provide: SystemApiService, useValue: systemApi },
      { provide: VehicleApiService, useValue: { getVehicle: () => of(detail) } }
    ]
  });
}

describe('FleetStateService', () => {
  it('keeps the last loaded fleet and exposes the problem title when a reload fails', () => {
    const problem = new HttpErrorResponse({
      status: 500,
      error: { title: 'El servicio de flota no está disponible.' }
    });
    configure({ getFleet: jasmine.createSpy('getFleet').and.returnValues(of(fleet), throwError(() => problem)) });
    const service = TestBed.inject(FleetStateService);

    service.load();
    service.load();

    expect(service.vehicles()).toEqual(fleet);
    expect(service.loadError()).toBe('El servicio de flota no está disponible.');
  });

  it('loads the open orders of every vehicle from its detail', () => {
    configure({ getFleet: () => of(fleet) });
    const service = TestBed.inject(FleetStateService);

    service.load();

    expect(service.openOrders()).toEqual([
      { orderId: 'order-1', vehicleId: 'vehicle-1', plate: 'ABC-123', routineName: 'Aceite', status: 'Created' }
    ]);
  });

  it('reloads the fleet and events when a command notifies a refresh', () => {
    const fleetApi = { getFleet: jasmine.createSpy('getFleet').and.returnValue(of(fleet)) };
    const systemApi = { getEvents: jasmine.createSpy('getEvents').and.returnValue(of([])) };
    configure(fleetApi, systemApi);
    const service = TestBed.inject(FleetStateService);
    TestBed.flushEffects();

    service.load();
    TestBed.inject(RefreshService).notifyCommandSucceeded();
    TestBed.flushEffects();

    expect(fleetApi.getFleet).toHaveBeenCalledTimes(2);
    expect(systemApi.getEvents).toHaveBeenCalledTimes(2);
  });
});
