import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { FleetApiService } from '../../core/api/fleet-api.service';
import { FleetVehicle } from '../../core/api/models/fleet.model';
import { RecordedEvent } from '../../core/api/models/recorded-event.model';
import { VehicleDetail } from '../../core/api/models/vehicle.model';
import { SystemApiService } from '../../core/api/system-api.service';
import { VehicleApiService } from '../../core/api/vehicle-api.service';
import { FleetPageComponent } from './fleet-page.component';

const fleet: FleetVehicle[] = [
  {
    id: 'vehicle-1',
    plate: 'ABC-123',
    description: 'Camión 3 t',
    odometer: 59000,
    awaitingQuote: false,
    routines: [
      { id: 'routine-oil', name: 'Aceite', status: 'DueSoon' },
      { id: 'routine-tyres', name: 'Llantas', status: 'UpToDate' }
    ]
  },
  {
    id: 'vehicle-2',
    plate: 'XYZ-789',
    description: 'Camión 5 t',
    odometer: 102100,
    awaitingQuote: false,
    routines: [{ id: 'routine-brakes', name: 'Frenos', status: 'Overdue' }]
  }
];

function detailOf(vehicle: FleetVehicle): VehicleDetail {
  return {
    id: vehicle.id,
    plate: vehicle.plate,
    description: vehicle.description,
    odometer: vehicle.odometer,
    workshopName: 'Taller Central',
    routines: vehicle.routines.map((routine) => ({
      ...routine,
      interval: 'Cada 6 meses',
      openOrder: routine.status === 'Overdue' ? { id: 'order-1', status: 'Created' } : null,
      baseline: { kilometers: vehicle.odometer, date: '2026-01-01' },
      remaining: { kilometers: 1000, days: 20 }
    }))
  };
}

const noEvents: RecordedEvent[] = [];

async function render(vehicles: FleetVehicle[], events: RecordedEvent[] = noEvents): Promise<HTMLElement> {
  await TestBed.configureTestingModule({
    imports: [FleetPageComponent],
    providers: [
      provideRouter([]),
      { provide: FleetApiService, useValue: { getFleet: () => of(vehicles) } },
      { provide: SystemApiService, useValue: { getEvents: () => of(events) } },
      {
        provide: VehicleApiService,
        useValue: { getVehicle: (id: string) => of(detailOf(vehicles.find((vehicle) => vehicle.id === id)!)) }
      }
    ]
  }).compileComponents();

  const fixture: ComponentFixture<FleetPageComponent> = TestBed.createComponent(FleetPageComponent);
  fixture.detectChanges();
  return fixture.nativeElement;
}

function texts(element: HTMLElement, selector: string): (string | undefined)[] {
  return Array.from(element.querySelectorAll(selector)).map((node) => node.textContent?.replace(/\s+/g, ' ').trim());
}

describe('FleetPageComponent', () => {
  it('shows one row per vehicle with plate, description, and odometer', async () => {
    const element = await render(fleet);
    const rows = element.querySelectorAll('.vehicle-row');

    expect(rows.length).toBe(2);
    expect(rows[0].querySelector('.vehicle-row__plate')?.textContent).toContain('ABC-123');
    expect(texts(element, '.vehicle-row__description')[0]).toBe('Camión 3 t·Odómetro 59.000 km');
  });

  it('titles the list "Mis vehículos" with the status subtitle and legend', async () => {
    const element = await render(fleet);

    expect(element.querySelector('h1')?.textContent).toContain('Mis vehículos');
    expect(element.textContent).toContain('El estado de cada rutina es el más urgente entre kilómetros y tiempo.');
    expect(texts(element, '.fleet-page__legend .status-chip')).toEqual(['Al día', 'Por vencer', 'Vencida']);
  });

  it('shows the four summary tiles', async () => {
    const element = await render(fleet);

    expect(texts(element, '.fleet-stat__value')).toEqual(['2', '1', '1', '1']);
    expect(texts(element, '.fleet-stat__label')).toEqual(['Vehículos', 'Por vencer', 'Vencida', 'Orden por cotizar']);
  });

  it('shows each routine as "{name} · {status}" chips', async () => {
    const element = await render(fleet);

    expect(texts(element, '.vehicle-row .status-chip')).toEqual([
      'Aceite · Por vencer',
      'Llantas · Al día',
      'Frenos · Vencida'
    ]);
  });

  it('colors the rail with the most urgent routine status', async () => {
    const element = await render(fleet);
    const rows = element.querySelectorAll('.vehicle-row');

    expect(rows[0].classList).toContain('vehicle-row--warning');
    expect(rows[1].classList).toContain('vehicle-row--danger');
  });

  it('shows the open-order line under vehicles that have one', async () => {
    const element = await render(fleet);

    expect(texts(element, '.vehicle-row__order')).toEqual(['Orden abierta · Frenos · Creada']);
  });

  it('shows the empty copy when the client has no vehicles', async () => {
    const element = await render([]);

    expect(element.textContent).toContain('Esta flota no tiene vehículos.');
  });

  it('links each row to its vehicle screen', async () => {
    const element = await render(fleet);
    const links = Array.from(element.querySelectorAll('a')).map((link) => link.getAttribute('href'));

    expect(links).toContain('/vehicles/vehicle-1');
  });
});
