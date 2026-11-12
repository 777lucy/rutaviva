import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { WorkshopApiService } from './workshop-api.service';

describe('WorkshopApiService (Contract)', () => {
  let service: WorkshopApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(WorkshopApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('POSTs the quote amount to /api/workshop/orders/{id}/quote', () => {
    service.submitQuote('order-1', 2000000).subscribe();

    const request = http.expectOne(`${environment.apiUrl}/api/workshop/orders/order-1/quote`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ amount: 2000000 });
    request.flush(null);
  });

  it('POSTs start to /api/workshop/orders/{id}/start without a body', () => {
    service.startService('order-1').subscribe();

    const request = http.expectOne(`${environment.apiUrl}/api/workshop/orders/order-1/start`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toBeNull();
    request.flush(null);
  });

  it('POSTs kilometers and service date to /api/workshop/orders/{id}/complete', () => {
    service.completeService('order-1', 60000, '2026-11-11').subscribe();

    const request = http.expectOne(`${environment.apiUrl}/api/workshop/orders/order-1/complete`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ kilometers: 60000, performedOn: '2026-11-11' });
    request.flush(null);
  });
});
