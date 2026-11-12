import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiUrl } from './api-url';
import { OrderDetail } from './models/order.model';

@Injectable({ providedIn: 'root' })
export class OrderApiService {
  private readonly http = inject(HttpClient);

  getOrder(orderId: string): Observable<OrderDetail> {
    return this.http.get<OrderDetail>(this.orderUrl(orderId));
  }

  approveOrder(orderId: string): Observable<void> {
    return this.http.post<void>(`${this.orderUrl(orderId)}/approve`, null);
  }

  rejectOrder(orderId: string): Observable<void> {
    return this.http.post<void>(`${this.orderUrl(orderId)}/reject`, null);
  }

  cancelOrder(orderId: string): Observable<void> {
    return this.http.post<void>(`${this.orderUrl(orderId)}/cancel`, null);
  }

  private orderUrl(orderId: string): string {
    return apiUrl(`/api/orders/${orderId}`);
  }
}
