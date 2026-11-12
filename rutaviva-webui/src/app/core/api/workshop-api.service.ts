import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiUrl } from './api-url';

@Injectable({ providedIn: 'root' })
export class WorkshopApiService {
  private readonly http = inject(HttpClient);

  submitQuote(orderId: string, amount: number): Observable<void> {
    return this.http.post<void>(this.orderUrl(orderId, 'quote'), { amount });
  }

  startService(orderId: string): Observable<void> {
    return this.http.post<void>(this.orderUrl(orderId, 'start'), null);
  }

  completeService(orderId: string, kilometers: number, performedOn: string): Observable<void> {
    return this.http.post<void>(this.orderUrl(orderId, 'complete'), { kilometers, performedOn });
  }

  private orderUrl(orderId: string, action: 'quote' | 'start' | 'complete'): string {
    return apiUrl(`/api/workshop/orders/${orderId}/${action}`);
  }
}
