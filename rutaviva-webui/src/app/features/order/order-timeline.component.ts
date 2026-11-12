import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { OrderDetail } from '../../core/api/models/order.model';
import { orderHistorySteps } from './order-history-copy';

@Component({
  selector: 'app-order-timeline',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './order-timeline.component.html',
  styleUrl: './order-timeline.component.scss'
})
export class OrderTimelineComponent {
  readonly order = input.required<OrderDetail>();

  protected readonly steps = computed(() => orderHistorySteps(this.order()));
}
