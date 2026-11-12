import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';
import { OrderApiService } from '../../core/api/order-api.service';
import { OrderDetail } from '../../core/api/models/order.model';
import { BreadcrumbService } from '../../shared/components/breadcrumb/breadcrumb.service';
import { OrderPageComponent } from './order-page.component';

type OrderApiMock = jasmine.SpyObj<OrderApiService>;

function orderWith(overrides: Partial<OrderDetail>): OrderDetail {
  return {
    id: 'order-1',
    vehicleId: 'vehicle-1',
    plate: 'XYZ-789',
    routineName: 'Frenos',
    workshopName: 'Taller Central',
    status: 'Created',
    quoteAmount: null,
    cancellationReason: null,
    completionKilometers: null,
    completionDate: null,
    history: [
      { name: 'Creada', detail: 'La rutina venció', done: true },
      { name: 'Cotizada', detail: 'Pendiente', done: false }
    ],
    ...overrides
  };
}

function apiMock(getOrder: () => Observable<OrderDetail>): OrderApiMock {
  const api = jasmine.createSpyObj<OrderApiService>('OrderApiService', [
    'getOrder',
    'approveOrder',
    'rejectOrder',
    'cancelOrder'
  ]);
  api.getOrder.and.callFake(getOrder);
  api.approveOrder.and.returnValue(of(undefined));
  api.rejectOrder.and.returnValue(of(undefined));
  api.cancelOrder.and.returnValue(of(undefined));
  return api;
}

async function renderWith(api: OrderApiMock, orderId: string): Promise<ComponentFixture<OrderPageComponent>> {
  await TestBed.configureTestingModule({
    imports: [OrderPageComponent],
    providers: [provideRouter([]), { provide: OrderApiService, useValue: api }]
  }).compileComponents();

  const fixture = TestBed.createComponent(OrderPageComponent);
  fixture.componentRef.setInput('id', orderId);
  fixture.detectChanges();
  return fixture;
}

async function render(order: OrderDetail): Promise<HTMLElement> {
  const fixture = await renderWith(apiMock(() => of(order)), order.id);
  return fixture.nativeElement;
}

async function renderFailing(getOrder: () => Observable<OrderDetail>): Promise<HTMLElement> {
  const fixture = await renderWith(apiMock(getOrder), 'order-missing');
  return fixture.nativeElement;
}

function actionButton(element: HTMLElement, label: string): HTMLButtonElement | undefined {
  return Array.from(element.querySelectorAll<HTMLButtonElement>('.order-page__action')).find(
    (button) => button.textContent?.trim() === label
  );
}

function texts(element: HTMLElement, selector: string): string[] {
  return Array.from(element.querySelectorAll(selector)).map((node) => node.textContent?.trim() ?? '');
}

describe('OrderPageComponent', () => {
  it('links back to the vehicle by plate', async () => {
    const element = await render(orderWith({}));
    const back = element.querySelector('.back-link');

    expect(back?.textContent).toContain('Volver a XYZ-789');
    expect(back?.getAttribute('href')).toBe('/vehicles/vehicle-1');
  });

  it('titles the page with the status chip and a routine, plate, and workshop subtitle', async () => {
    const element = await render(orderWith({}));

    expect(element.querySelector('h1')?.textContent?.trim()).toBe('Orden de servicio');
    expect(element.querySelector('.order-page__title .status-chip')?.textContent?.trim()).toBe('Creada');
    expect(element.querySelector('.order-page__subtitle')?.textContent?.trim()).toBe(
      'Frenos · XYZ-789 · Taller Central'
    );
  });

  it('adds the plate and the order to the breadcrumb', async () => {
    await render(orderWith({}));

    expect(TestBed.inject(BreadcrumbService).crumbs()).toEqual([
      { label: 'Flota', link: ['/'] },
      { label: 'XYZ-789', link: ['/vehicles', 'vehicle-1'] },
      { label: 'Orden', link: null }
    ]);
  });

  it('lists the history steps with their UI copy and done state', async () => {
    const element = await render(orderWith({}));

    expect(texts(element, '.order-timeline__name')).toEqual(['Creada', 'Cotizada']);
    expect(texts(element, '.order-timeline__detail')).toEqual([
      'La rutina venció',
      'Esperando la cotización del taller'
    ]);
    expect(element.querySelectorAll('.order-timeline__step--done').length).toBe(1);
  });

  it('shows the quote amount and the two-line approval alert when the order is quoted', async () => {
    const quoted = await render(orderWith({ status: 'Quoted', quoteAmount: 2000000 }));

    expect(quoted.querySelector('.order-page__amount')?.textContent?.trim()).toBe('$ 2.000.000');
    expect(texts(quoted, '.order-page__approval p')).toEqual([
      'Requiere su aprobación porque supera 1.500.000 COP.',
      'Un valor igual o menor se aprueba sin pasar por usted.'
    ]);
  });

  it('offers approve, reject, and cancel with the actions note for a quoted order', async () => {
    const quoted = await render(orderWith({ status: 'Quoted', quoteAmount: 2000000 }));

    expect(texts(quoted, '.order-page__action')).toEqual(['Aprobar', 'Rechazar', 'Cancelar']);
    expect(quoted.querySelector('.order-page__note')?.textContent?.trim()).toBe(
      'Rechazar cancela la orden. Cancelar deja de estar disponible cuando el taller ya inició el servicio.'
    );
  });

  it('does not show the approval alert for an approved order', async () => {
    const approved = await render(orderWith({ status: 'Approved', quoteAmount: 900000 }));

    expect(approved.querySelector('.order-page__notice')?.textContent?.trim()).toBe('Aprobada');
    expect(approved.querySelector('.order-page__approval')).toBeNull();
  });

  it('tells the client the workshop has not quoted a created order yet', async () => {
    const element = await render(orderWith({ status: 'Created' }));

    expect(element.textContent).toContain('Esperando la cotización del taller');
  });

  it('tells the client the service is in progress', async () => {
    const element = await render(orderWith({ status: 'InProgress', quoteAmount: 900000 }));

    expect(element.textContent).toContain('El taller ya inició el servicio');
  });

  it('shows reported kilometers and date in the history when completed', async () => {
    const element = await render(
      orderWith({
        status: 'Completed',
        quoteAmount: 900000,
        completionKilometers: 60000,
        completionDate: '2026-11-11',
        history: [{ name: 'Completada', detail: 'Servicio reportado', done: true }]
      })
    );

    expect(texts(element, '.order-timeline__detail')).toEqual(['60.000 km · 11/11/2026']);
    expect(element.textContent).toContain('La rutina volvió a estar al día');
    expect(element.querySelector('.order-page__action')).toBeNull();
  });

  it('shows the problem title and a link back to the fleet when the order is not found', async () => {
    const notFound = new HttpErrorResponse({ status: 404, error: { title: 'No se encontró la orden.' } });
    const element = await renderFailing(() => throwError(() => notFound));

    expect(element.querySelector('[role="alert"]')?.textContent).toContain('No se encontró la orden.');
    expect(element.querySelector('.back-link')?.getAttribute('href')).toBe('/');
  });

  it('shows the cancellation reason for a rejected quote', async () => {
    const element = await render(orderWith({ status: 'Cancelled', cancellationReason: 'QuoteRejected' }));

    expect(element.textContent).toContain('Cotización rechazada');
  });

  it('shows that the client cancelled the order', async () => {
    const element = await render(orderWith({ status: 'Cancelled', cancellationReason: 'CancelledByClient' }));

    expect(element.textContent).toContain('Cancelada por el cliente');
  });

  describe('client actions', () => {
    it('approves the quote after confirmation and reloads the order', async () => {
      const api = apiMock(() => of(orderWith({ status: 'Quoted', quoteAmount: 2000000 })));
      const fixture = await renderWith(api, 'order-1');

      actionButton(fixture.nativeElement, 'Aprobar')?.click();
      fixture.nativeElement.querySelector('.confirm-dialog__confirm').click();
      TestBed.flushEffects();

      expect(api.approveOrder).toHaveBeenCalledWith('order-1');
      expect(api.getOrder).toHaveBeenCalledTimes(2);
    });

    it('rejects the quote after confirmation and explains the order is not replaced', async () => {
      const api = apiMock(() => of(orderWith({ status: 'Quoted', quoteAmount: 2000000 })));
      const fixture = await renderWith(api, 'order-1');

      actionButton(fixture.nativeElement, 'Rechazar')?.click();
      fixture.detectChanges();

      const dialog = fixture.nativeElement.querySelector('.confirm-dialog');
      expect(dialog?.querySelector('.confirm-dialog__title')?.textContent?.trim()).toBe('¿Rechazar la cotización?');
      expect(dialog?.textContent).toContain('Esta versión no abre otra orden.');
      fixture.nativeElement.querySelector('.confirm-dialog__confirm').click();

      expect(api.rejectOrder).toHaveBeenCalledWith('order-1');
    });

    it('cancels the order after confirmation', async () => {
      const api = apiMock(() => of(orderWith({ status: 'Created' })));
      const fixture = await renderWith(api, 'order-1');

      actionButton(fixture.nativeElement, 'Cancelar')?.click();
      fixture.nativeElement.querySelector('.confirm-dialog__confirm').click();

      expect(api.cancelOrder).toHaveBeenCalledWith('order-1');
    });

    it('does not call the API when the confirmation is dismissed', async () => {
      const api = apiMock(() => of(orderWith({ status: 'Quoted', quoteAmount: 2000000 })));
      const fixture = await renderWith(api, 'order-1');

      actionButton(fixture.nativeElement, 'Aprobar')?.click();
      fixture.nativeElement.querySelector('.confirm-dialog__cancel').click();

      expect(api.approveOrder).not.toHaveBeenCalled();
      expect(api.getOrder).toHaveBeenCalledTimes(1);
    });
  });
});
