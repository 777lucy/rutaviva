import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { RecordedEvent } from './models/recorded-event.model';
import { SystemApiService } from './system-api.service';

describe('SystemApiService (Contract)', () => {
  let service: SystemApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(SystemApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('POSTs the review date to /api/system/daily-review', () => {
    service.runDailyReview('2026-11-11').subscribe();

    const request = http.expectOne(`${environment.apiUrl}/api/system/daily-review`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ date: '2026-11-11' });
    request.flush([]);
  });

  it('GETs the newest-first domain events from /api/system/events', () => {
    const payload: RecordedEvent[] = [
      {
        name: 'ServiceOrderOpened',
        title: 'Orden abierta',
        vehicleId: 'vehicle-1',
        routineId: 'routine-1',
        orderId: 'order-1',
        occurredAt: '2026-11-11T13:00:00Z'
      }
    ];
    let events: RecordedEvent[] | undefined;

    service.getEvents().subscribe((result) => (events = result));
    const request = http.expectOne(`${environment.apiUrl}/api/system/events`);
    expect(request.request.method).toBe('GET');
    request.flush(payload);

    expect(events).toEqual(payload);
  });
});
