import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Subject, of, throwError } from 'rxjs';
import { FleetApiService } from '../../core/api/fleet-api.service';
import { SystemApiService } from '../../core/api/system-api.service';
import { TelemetryApiService } from '../../core/api/telemetry-api.service';
import { VehicleApiService } from '../../core/api/vehicle-api.service';
import { WorkshopApiService } from '../../core/api/workshop-api.service';
import { FleetVehicle } from '../../core/api/models/fleet.model';
import { RecordedEvent } from '../../core/api/models/recorded-event.model';
import { ServiceOrderStatus } from '../../core/api/models/status.model';
import { ReadingOutcome } from '../../core/api/models/telemetry.model';
import { OpenOrder, VehicleDetail, VehicleRoutine } from '../../core/api/models/vehicle.model';
import { todayInBogota } from './bogota-today';
import { SimulationPanelComponent } from './simulation-panel.component';

interface SimulationApis {
  fleet: jasmine.SpyObj<FleetApiService>;
  vehicle: jasmine.SpyObj<VehicleApiService>;
  workshop: jasmine.SpyObj<WorkshopApiService>;
  telemetry: jasmine.SpyObj<TelemetryApiService>;
  system: jasmine.SpyObj<SystemApiService>;
}

function createApis(): SimulationApis {
  const apis: SimulationApis = {
    fleet: jasmine.createSpyObj<FleetApiService>('FleetApiService', ['getFleet']),
    vehicle: jasmine.createSpyObj<VehicleApiService>('VehicleApiService', ['getVehicle']),
    workshop: jasmine.createSpyObj<WorkshopApiService>('WorkshopApiService', [
      'submitQuote',
      'startService',
      'completeService'
    ]),
    telemetry: jasmine.createSpyObj<TelemetryApiService>('TelemetryApiService', ['registerReading']),
    system: jasmine.createSpyObj<SystemApiService>('SystemApiService', ['runDailyReview', 'getEvents'])
  };
  apis.fleet.getFleet.and.returnValue(of([]));
  apis.workshop.submitQuote.and.returnValue(of(undefined));
  apis.workshop.startService.and.returnValue(of(undefined));
  apis.workshop.completeService.and.returnValue(of(undefined));
  apis.telemetry.registerReading.and.returnValue(of({ result: 'Accepted' }));
  apis.system.getEvents.and.returnValue(of([]));
  apis.system.runDailyReview.and.returnValue(of([]));
  return apis;
}

function fleetVehicle(id: string, plate: string): FleetVehicle {
  return { id, plate, description: 'Camión 3.5 t', odometer: 60000, awaitingQuote: false, routines: [] };
}

function routine(name: string, openOrder: OpenOrder | null): VehicleRoutine {
  return {
    id: `${name}-id`,
    name,
    interval: '10.000 km',
    status: 'Overdue',
    openOrder,
    baseline: { kilometers: 50000, date: '2026-01-01' },
    remaining: { kilometers: -1000, days: null }
  };
}

function vehicleDetail(id: string, plate: string, routines: VehicleRoutine[]): VehicleDetail {
  return { id, plate, description: 'Camión 3.5 t', odometer: 60000, workshopName: 'Taller Central', routines };
}

function apisWithOrders(routines: VehicleRoutine[]): SimulationApis {
  const apis = createApis();
  apis.fleet.getFleet.and.returnValue(of([fleetVehicle('vehicle-1', 'XYZ-789')]));
  apis.vehicle.getVehicle.and.returnValue(of(vehicleDetail('vehicle-1', 'XYZ-789', routines)));
  return apis;
}

function apisWithOrder(status: ServiceOrderStatus): SimulationApis {
  return apisWithOrders([routine('Frenos', { id: 'order-1', status }), routine('Aceite', null)]);
}

async function renderPanel(apis: SimulationApis): Promise<ComponentFixture<SimulationPanelComponent>> {
  await TestBed.configureTestingModule({
    imports: [SimulationPanelComponent],
    providers: [
      { provide: FleetApiService, useValue: apis.fleet },
      { provide: VehicleApiService, useValue: apis.vehicle },
      { provide: WorkshopApiService, useValue: apis.workshop },
      { provide: TelemetryApiService, useValue: apis.telemetry },
      { provide: SystemApiService, useValue: apis.system }
    ]
  }).compileComponents();

  const fixture = TestBed.createComponent(SimulationPanelComponent);
  fixture.detectChanges();
  return fixture;
}

function queryRequired<T extends Element>(element: HTMLElement, selector: string): T {
  const found = element.querySelector<T>(selector);
  if (!found) {
    throw new Error(`Missing element: ${selector}`);
  }
  return found;
}

function enabledWorkshopActions(element: HTMLElement): string[] {
  return Array.from(element.querySelectorAll<HTMLButtonElement>('.workshop-section__action'))
    .filter((button) => !button.disabled)
    .map((button) => button.textContent?.trim() ?? '');
}

function recordedEvent(title: string, overrides: Partial<RecordedEvent> = {}): RecordedEvent {
  return {
    name: 'RoutineBecameDueSoon',
    title,
    vehicleId: 'vehicle-1',
    routineId: null,
    orderId: null,
    occurredAt: '2026-11-11T14:00:00Z',
    ...overrides
  };
}

function eventLines(element: HTMLElement): string[] {
  return Array.from(element.querySelectorAll<HTMLElement>('.event-list__line')).map(
    (line) => line.textContent?.trim() ?? ''
  );
}

function cardTitles(element: HTMLElement): string[] {
  return Array.from(element.querySelectorAll<HTMLElement>('.sim-block h3')).map((title) => title.textContent?.trim() ?? '');
}

function submitReading(fixture: ComponentFixture<SimulationPanelComponent>, kilometers: string): void {
  queryRequired<HTMLInputElement>(fixture.nativeElement, '.reading-form__kilometers').value = kilometers;
  queryRequired<HTMLButtonElement>(fixture.nativeElement, '.reading-form__submit').click();
  fixture.detectChanges();
  TestBed.flushEffects();
}

function clickWorkshopButton(element: HTMLElement, label: string): void {
  const button = Array.from(element.querySelectorAll<HTMLButtonElement>('.workshop-section__action')).find(
    (candidate) => candidate.textContent?.trim() === label
  );
  button?.click();
}

describe('SimulationPanelComponent', () => {
  describe('workshop orders', () => {
    it('lists open orders with plate, routine, and Spanish status', async () => {
      const fixture = await renderPanel(apisWithOrder('Created'));
      const options = Array.from(fixture.nativeElement.querySelectorAll('.workshop-section__order option')).map(
        (option) => (option as HTMLOptionElement).textContent?.trim()
      );

      expect(options).toEqual(['XYZ-789 · Frenos · Creada']);
    });

    it('shows the explanatory note under the panel label', async () => {
      const fixture = await renderPanel(createApis());

      expect(fixture.nativeElement.querySelector('.simulation-panel__note')?.textContent?.trim()).toBe(
        'No son acciones del cliente.'
      );
    });

    it('titles the cards and names the workshop card after the workshop', async () => {
      const fixture = await renderPanel(apisWithOrder('Created'));

      expect(cardTitles(fixture.nativeElement)).toEqual([
        'Telemetría',
        'Revisión diaria',
        'Taller Central',
        'Eventos recientes'
      ]);
    });

    it('asks for completion kilometers and date only while the order is in progress', async () => {
      const approved = await renderPanel(apisWithOrder('Approved'));

      expect(approved.nativeElement.querySelector('.workshop-section__kilometers')).toBeNull();
      expect(approved.nativeElement.querySelector('.workshop-section__date')).toBeNull();
    });

    it('enables only "Enviar cotización" for a created order', async () => {
      const fixture = await renderPanel(apisWithOrder('Created'));

      expect(enabledWorkshopActions(fixture.nativeElement)).toEqual(['Enviar cotización']);
    });

    it('enables only "Iniciar" for an approved order', async () => {
      const fixture = await renderPanel(apisWithOrder('Approved'));

      expect(enabledWorkshopActions(fixture.nativeElement)).toEqual(['Iniciar']);
    });

    it('enables only "Completar" for an order in progress', async () => {
      const fixture = await renderPanel(apisWithOrder('InProgress'));

      expect(enabledWorkshopActions(fixture.nativeElement)).toEqual(['Completar']);
    });

    it('lists only orders the workshop can act on', async () => {
      const fixture = await renderPanel(apisWithOrder('Quoted'));
      const element: HTMLElement = fixture.nativeElement;

      expect(element.querySelectorAll('.workshop-section__order option').length).toBe(0);
      expect(element.textContent).toContain('No hay órdenes para el taller.');
    });

    it('sends the typed amount when quoting the selected order', async () => {
      const apis = apisWithOrder('Created');
      const fixture = await renderPanel(apis);
      queryRequired<HTMLInputElement>(fixture.nativeElement, '.workshop-section__amount').value = '900000';

      clickWorkshopButton(fixture.nativeElement, 'Enviar cotización');

      expect(apis.workshop.submitQuote).toHaveBeenCalledWith('order-1', 900000);
    });

    it('starts the approved service', async () => {
      const apis = apisWithOrder('Approved');
      const fixture = await renderPanel(apis);

      clickWorkshopButton(fixture.nativeElement, 'Iniciar');

      expect(apis.workshop.startService).toHaveBeenCalledWith('order-1');
    });

    it('defaults the completion date to today in Bogotá', async () => {
      const fixture = await renderPanel(apisWithOrder('InProgress'));

      expect(queryRequired<HTMLInputElement>(fixture.nativeElement, '.workshop-section__date').value).toBe(
        todayInBogota(new Date())
      );
    });

    it('completes the service with the typed kilometers and date', async () => {
      const apis = apisWithOrder('InProgress');
      const fixture = await renderPanel(apis);
      queryRequired<HTMLInputElement>(fixture.nativeElement, '.workshop-section__kilometers').value = '61000';
      queryRequired<HTMLInputElement>(fixture.nativeElement, '.workshop-section__date').value = '2026-11-10';

      clickWorkshopButton(fixture.nativeElement, 'Completar');

      expect(apis.workshop.completeService).toHaveBeenCalledWith('order-1', 61000, '2026-11-10');
    });

    it('acts on the order chosen in the selector', async () => {
      const apis = apisWithOrders([
        routine('Frenos', { id: 'order-1', status: 'Created' }),
        routine('Aceite', { id: 'order-2', status: 'Approved' })
      ]);
      const fixture = await renderPanel(apis);
      const selector = queryRequired<HTMLSelectElement>(fixture.nativeElement, '.workshop-section__order');
      selector.value = 'order-2';
      selector.dispatchEvent(new Event('change'));
      fixture.detectChanges();

      clickWorkshopButton(fixture.nativeElement, 'Iniciar');

      expect(apis.workshop.startService).toHaveBeenCalledWith('order-2');
    });

    it('notifies a refresh that reloads the open orders after a workshop command', async () => {
      const apis = apisWithOrder('Approved');
      const fixture = await renderPanel(apis);

      clickWorkshopButton(fixture.nativeElement, 'Iniciar');
      TestBed.flushEffects();

      expect(apis.fleet.getFleet).toHaveBeenCalledTimes(2);
    });
  });

  describe('workshop conflicts', () => {
    it('shows the problem title on a 409 and keeps the last loaded order list', async () => {
      const apis = apisWithOrder('Approved');
      const conflict = new HttpErrorResponse({ status: 409, error: { title: 'La orden ya no está aprobada.' } });
      apis.workshop.startService.and.returnValue(throwError(() => conflict));
      const fixture = await renderPanel(apis);

      clickWorkshopButton(fixture.nativeElement, 'Iniciar');
      fixture.detectChanges();

      const element: HTMLElement = fixture.nativeElement;
      expect(queryRequired<HTMLElement>(element, '.workshop-section__error').textContent).toContain(
        'La orden ya no está aprobada.'
      );
      expect(element.querySelectorAll('.workshop-section__order option').length).toBe(1);
    });
  });

  describe('readings, daily review, and events', () => {
    it('sends the typed kilometers for the first vehicle', async () => {
      const apis = apisWithOrder('Created');
      const fixture = await renderPanel(apis);

      submitReading(fixture, '61000');

      expect(apis.telemetry.registerReading).toHaveBeenCalledWith('vehicle-1', 61000);
    });

    it('adds an ignored reading to the top of the events and does not refresh', async () => {
      const apis = apisWithOrder('Created');
      apis.system.getEvents.and.returnValue(
        of([recordedEvent('Rutina por vencer', { occurredAt: '2020-01-01T00:00:00Z' })])
      );
      apis.telemetry.registerReading.and.returnValue(of({ result: 'Ignored' }));
      const fixture = await renderPanel(apis);

      submitReading(fixture, '1000');

      expect(eventLines(fixture.nativeElement)).toEqual(['Lectura ignorada · XYZ-789', 'Rutina por vencer · XYZ-789']);
      expect(apis.fleet.getFleet).toHaveBeenCalledTimes(1);
    });

    it('reloads fleet and events after an accepted reading', async () => {
      const apis = apisWithOrder('Created');
      const fixture = await renderPanel(apis);

      submitReading(fixture, '61000');

      expect(apis.fleet.getFleet).toHaveBeenCalledTimes(2);
      expect(apis.system.getEvents).toHaveBeenCalledTimes(2);
    });

    it('disables the reading control until the request finishes', async () => {
      const apis = apisWithOrder('Created');
      apis.telemetry.registerReading.and.returnValue(new Subject<ReadingOutcome>());
      const fixture = await renderPanel(apis);

      submitReading(fixture, '61000');

      expect(queryRequired<HTMLButtonElement>(fixture.nativeElement, '.reading-form__submit').disabled).toBeTrue();
    });

    it('defaults the daily review date to today in Bogotá', async () => {
      const fixture = await renderPanel(createApis());

      expect(queryRequired<HTMLInputElement>(fixture.nativeElement, '.review-form__date').value).toBe(
        todayInBogota(new Date())
      );
    });

    it('runs the daily review for the chosen date', async () => {
      const apis = createApis();
      const fixture = await renderPanel(apis);
      queryRequired<HTMLInputElement>(fixture.nativeElement, '.review-form__date').value = '2026-11-10';

      queryRequired<HTMLButtonElement>(fixture.nativeElement, '.review-form__submit').click();

      expect(apis.system.runDailyReview).toHaveBeenCalledWith('2026-11-10');
    });

    it('notifies a refresh that reloads fleet and events after the daily review', async () => {
      const apis = apisWithOrder('Created');
      const fixture = await renderPanel(apis);

      queryRequired<HTMLButtonElement>(fixture.nativeElement, '.review-form__submit').click();
      TestBed.flushEffects();

      expect(apis.fleet.getFleet).toHaveBeenCalledTimes(2);
      expect(apis.system.getEvents).toHaveBeenCalledTimes(2);
    });

    it('lists recorded event titles in the order the API returns them, newest first', async () => {
      const apis = createApis();
      apis.system.getEvents.and.returnValue(of([recordedEvent('Servicio completado'), recordedEvent('Rutina por vencer')]));
      const fixture = await renderPanel(apis);

      expect(eventLines(fixture.nativeElement)).toEqual(['Servicio completado', 'Rutina por vencer']);
    });

    it('names the plate and the routine of each event without a timestamp', async () => {
      const apis = apisWithOrder('Created');
      apis.system.getEvents.and.returnValue(of([recordedEvent('Orden abierta', { routineId: 'Frenos-id' })]));
      const fixture = await renderPanel(apis);
      const element: HTMLElement = fixture.nativeElement;

      expect(eventLines(element)).toEqual(['Orden abierta · XYZ-789 Frenos']);
      expect(element.querySelector('.event-list time')).toBeNull();
    });

    it('shows only the 8 most recent events', async () => {
      const apis = createApis();
      const titles = Array.from({ length: 12 }, (_, index) => `Evento ${index + 1}`);
      apis.system.getEvents.and.returnValue(of(titles.map((title) => recordedEvent(title))));
      const fixture = await renderPanel(apis);

      expect(eventLines(fixture.nativeElement)).toEqual(titles.slice(0, 8));
    });
  });
});
