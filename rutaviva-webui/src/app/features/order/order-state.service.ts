import { Injectable, inject, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { OrderApiService } from '../../core/api/order-api.service';
import { OrderDetail } from '../../core/api/models/order.model';
import { problemTitle } from '../../core/problem-title';
import { RefreshService, reloadOnRefresh } from '../../core/refresh.service';
import { ClientAction } from './order-actions';

@Injectable()
export class OrderStateService {
  private readonly orderApi = inject(OrderApiService);
  private readonly refresh = inject(RefreshService);
  private orderId: string | null = null;

  readonly detail = signal<OrderDetail | null>(null);
  readonly loadError = signal<string | null>(null);

  constructor() {
    reloadOnRefresh(this.refresh, () => this.reload());
  }

  load(orderId: string): void {
    this.orderId = orderId;
    this.orderApi.getOrder(orderId).subscribe({
      next: (order) => {
        this.detail.set(order);
        this.loadError.set(null);
      },
      error: (error: unknown) => this.loadError.set(problemTitle(error))
    });
  }

  runAction(orderId: string, action: ClientAction): void {
    this.callAction(orderId, action).subscribe({
      next: () => this.refresh.notifyCommandSucceeded(),
      error: (error: unknown) => this.loadError.set(problemTitle(error))
    });
  }

  private reload(): void {
    if (this.orderId) {
      this.load(this.orderId);
    }
  }

  private callAction(orderId: string, action: ClientAction): Observable<void> {
    switch (action) {
      case 'approve':
        return this.orderApi.approveOrder(orderId);
      case 'reject':
        return this.orderApi.rejectOrder(orderId);
      case 'cancel':
        return this.orderApi.cancelOrder(orderId);
    }
  }
}
