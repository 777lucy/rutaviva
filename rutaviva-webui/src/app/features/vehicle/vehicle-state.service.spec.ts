import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { VehicleApiService } from '../../core/api/vehicle-api.service';
import { VehicleDetail } from '../../core/api/models/vehicle.model';
import { RefreshService } from '../../core/refresh.service';
import { VehicleStateService } from './vehicle-state.service';

const vehicle: VehicleDetail = {
  id: 'vehicle-1',
  plate: 'ABC-123',
  description: 'Camión',
  odometer: 58000,
  workshopName: 'Taller Central',
  routines: []
};

describe('VehicleStateService', () => {
  it('reloads the last requested vehicle when a command notifies a refresh', () => {
    const vehicleApi = { getVehicle: jasmine.createSpy('getVehicle').and.returnValue(of(vehicle)) };
    TestBed.configureTestingModule({
      providers: [VehicleStateService, { provide: VehicleApiService, useValue: vehicleApi }]
    });
    const service = TestBed.inject(VehicleStateService);
    TestBed.flushEffects();

    service.load('vehicle-1');
    TestBed.inject(RefreshService).notifyCommandSucceeded();
    TestBed.flushEffects();

    expect(vehicleApi.getVehicle).toHaveBeenCalledTimes(2);
    expect(vehicleApi.getVehicle).toHaveBeenCalledWith('vehicle-1');
  });
});
