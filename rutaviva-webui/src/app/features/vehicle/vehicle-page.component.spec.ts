import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';
import { VehicleApiService } from '../../core/api/vehicle-api.service';
import { VehicleDetail } from '../../core/api/models/vehicle.model';
import { BreadcrumbService } from '../../shared/components/breadcrumb/breadcrumb.service';
import { VehiclePageComponent } from './vehicle-page.component';

const vehicle: VehicleDetail = {
  id: 'vehicle-1',
  plate: 'ABC-123',
  description: 'Camión 3 ejes',
  odometer: 59000,
  workshopName: 'Taller Central',
  routines: [
    {
      id: 'routine-oil',
      name: 'Aceite',
      interval: 'cada 10.000 km o 6 meses',
      status: 'DueSoon',
      openOrder: { id: 'order-1', status: 'Created' },
      baseline: { kilometers: 50000, date: '2026-01-01' },
      remaining: { kilometers: 1000, days: 20 }
    },
    {
      id: 'routine-tyres',
      name: 'Llantas',
      interval: 'cada 15.000 km',
      status: 'UpToDate',
      openOrder: null,
      baseline: { kilometers: 48000, date: '2026-03-15' },
      remaining: { kilometers: 4000, days: null }
    }
  ]
};

async function render(getVehicle: () => Observable<VehicleDetail> = () => of(vehicle)): Promise<HTMLElement> {
  await TestBed.configureTestingModule({
    imports: [VehiclePageComponent],
    providers: [provideRouter([]), { provide: VehicleApiService, useValue: { getVehicle } }]
  }).compileComponents();

  const fixture: ComponentFixture<VehiclePageComponent> = TestBed.createComponent(VehiclePageComponent);
  fixture.componentRef.setInput('id', 'vehicle-1');
  fixture.detectChanges();
  return fixture.nativeElement;
}

function texts(element: HTMLElement, selector: string): string[] {
  return Array.from(element.querySelectorAll(selector)).map((node) => node.textContent?.trim() ?? '');
}

describe('VehiclePageComponent', () => {
  it('shows a back link to the fleet and the plate as title', async () => {
    const element = await render();
    const back = element.querySelector('.back-link');

    expect(back?.textContent).toContain('Volver a la flota');
    expect(back?.getAttribute('href')).toBe('/');
    expect(element.querySelector('h1')?.textContent?.trim()).toBe('ABC-123');
  });

  it('shows description, odometer, and workshop as pills', async () => {
    const element = await render();

    expect(texts(element, '.vehicle-page__pill')).toEqual([
      'Camión 3 ejes',
      'Odómetro 59.000 km',
      'Taller Central'
    ]);
  });

  it('shows the routine table header columns', async () => {
    const element = await render();

    expect(texts(element, '.routine-table__head span')).toEqual([
      'Rutina',
      'Intervalo',
      'Base',
      'Faltante',
      'Estado',
      'Orden abierta'
    ]);
  });

  it('shows the baseline and what remains for each routine', async () => {
    const element = await render();
    const rows = element.querySelectorAll('.routine-row');

    expect(rows[0].querySelector('.routine-row__baseline')?.textContent?.trim()).toBe('50.000 km · 1 ene 2026');
    expect(rows[0].querySelector('.routine-row__remaining')?.textContent?.trim()).toBe('1.000 km · 20 días');
    expect(rows[1].querySelector('.routine-row__baseline')?.textContent?.trim()).toBe('48.000 km · 15 mar 2026');
    expect(rows[1].querySelector('.routine-row__remaining')?.textContent?.trim()).toBe('4.000 km');
  });

  it('shows each routine with its interval text and status chip', async () => {
    const element = await render();
    const rows = element.querySelectorAll('.routine-row');

    expect(rows[0].querySelector('.routine-row__name')?.textContent?.trim()).toBe('Aceite');
    expect(rows[0].querySelector('.routine-row__interval')?.textContent?.trim()).toBe('cada 10.000 km o 6 meses');
    expect(rows[0].querySelector('.status-chip')?.textContent?.trim()).toBe('Por vencer');
  });

  it('links an open order with "Ver orden"', async () => {
    const element = await render();
    const orderLink = element.querySelector<HTMLAnchorElement>('.routine-row__order');

    expect(orderLink?.textContent).toContain('Ver orden');
    expect(orderLink?.getAttribute('href')).toBe('/orders/order-1');
  });

  it('shows "Sin orden abierta" for a routine without an open order', async () => {
    const element = await render();
    const rows = element.querySelectorAll('.routine-row');

    expect(rows[1].querySelector('.routine-row__order')).toBeNull();
    expect(rows[1].querySelector('.routine-row__no-order')?.textContent?.trim()).toBe('Sin orden abierta');
  });

  it('explains that finished orders are no longer shown', async () => {
    const element = await render();

    expect(element.querySelector('.vehicle-page__footnote')?.textContent?.trim()).toBe(
      'Una orden completada o cancelada deja de mostrarse. La fila queda solo con el estado de la rutina.'
    );
  });

  it('adds the plate to the breadcrumb', async () => {
    await render();

    expect(TestBed.inject(BreadcrumbService).crumbs().map((crumb) => crumb.label)).toEqual(['Flota', 'ABC-123']);
  });

  it('shows the problem title and a link back to the fleet when the vehicle is not found', async () => {
    const notFound = new HttpErrorResponse({ status: 404, error: { title: 'No se encontró el vehículo.' } });
    const element = await render(() => throwError(() => notFound));

    expect(element.querySelector('[role="alert"]')?.textContent).toContain('No se encontró el vehículo.');
    expect(element.querySelector('.back-link')?.getAttribute('href')).toBe('/');
  });
});
