import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { OrderDetail } from './models/order.model';
import { OrderApiService } from './order-api.service';

describe('OrderApiService (Contract)', () => {
  let service: OrderApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(OrderApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('GETs one order from /api/orders/{id} and returns its detail', () => {
    const payload: OrderDetail = {
      id: 'order-1',
      vehicleId: 'vehicle-1',
      plate: 'XYZ-789',
      routineName: 'Frenos',
      workshopName: 'Taller Central',
      status: 'Quoted',
      quoteAmount: 2000000,
      cancellationReason: null,
      completionKilometers: null,
      completionDate: null,
      history: [{ name: 'Creada', detail: 'La rutina venció', done: true }]
    };
    let order: OrderDetail | undefined;

    service.getOrder('order-1').subscribe((result) => (order = result));
    const request = http.expectOne(`${environment.apiUrl}/api/orders/order-1`);
    expect(request.request.method).toBe('GET');
    request.flush(payload);

    expect(order).toEqual(payload);
  });

  it('POSTs approve, reject, and cancel to their routes without a request body', () => {
    service.approveOrder('order-1').subscribe();
    service.rejectOrder('order-1').subscribe();
    service.cancelOrder('order-1').subscribe();

    const approve = http.expectOne(`${environment.apiUrl}/api/orders/order-1/approve`);
    const reject = http.expectOne(`${environment.apiUrl}/api/orders/order-1/reject`);
    const cancel = http.expectOne(`${environment.apiUrl}/api/orders/order-1/cancel`);

    expect(approve.request.method).toBe('POST');
    expect(reject.request.method).toBe('POST');
    expect(cancel.request.method).toBe('POST');
    expect(approve.request.body).toBeNull();
    expect(reject.request.body).toBeNull();
    expect(cancel.request.body).toBeNull();

    approve.flush(null);
    reject.flush(null);
    cancel.flush(null);
  });
});
