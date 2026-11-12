import { ChangeDetectionStrategy, Component, ElementRef, computed, inject, signal, viewChild } from '@angular/core';
import { todayInBogota } from '../bogota-today';
import { SimulationStateService } from '../simulation-state.service';
import { DefaultWorkshopTitle, WORKSHOP_ACTION_LABELS } from '../simulation-copy';
import { WorkshopAction, workshopActions } from '../workshop-actions';
import { statusLabel } from '../../../shared/status-label';

@Component({
  selector: 'app-workshop-section',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './workshop-section.component.html',
  styleUrl: './workshop-section.component.scss'
})
export class WorkshopSectionComponent {
  protected readonly state = inject(SimulationStateService);
  protected readonly statusLabel = statusLabel;
  protected readonly labels = WORKSHOP_ACTION_LABELS;
  protected readonly today = todayInBogota(new Date());

  private readonly selectedOrderId = signal<string | null>(null);
  private readonly kilometersInput = viewChild<ElementRef<HTMLInputElement>>('kilometers');
  private readonly performedOnInput = viewChild<ElementRef<HTMLInputElement>>('performedOn');

  protected readonly workshopOrders = computed(() =>
    this.state.openOrders().filter((order) => workshopActions(order.status).length > 0)
  );

  protected readonly selectedOrder = computed(() => {
    const orders = this.workshopOrders();
    return orders.find((order) => order.orderId === this.selectedOrderId()) ?? orders[0] ?? null;
  });

  protected readonly selectedOrderKey = computed(() => this.selectedOrder()?.orderId ?? '');

  protected readonly workshopTitle = computed(() => {
    const details = this.state.details();
    const vehicleId = this.selectedOrder()?.vehicleId;
    const vehicle = details.find((detail) => detail.id === vehicleId) ?? details[0];
    return vehicle?.workshopName || DefaultWorkshopTitle;
  });

  private readonly availableActions = computed(() => {
    const order = this.selectedOrder();
    return order ? workshopActions(order.status) : [];
  });

  protected readonly isCompleting = computed(() => this.availableActions().includes('complete'));

  protected isEnabled(action: WorkshopAction): boolean {
    return !this.state.workshopBusy() && this.availableActions().includes(action);
  }

  protected selectOrder(event: Event): void {
    this.selectedOrderId.set((event.target as HTMLSelectElement).value);
  }

  protected quote(amount: number): void {
    const order = this.selectedOrder();
    if (order) {
      this.state.quote(order.orderId, amount);
    }
  }

  protected start(): void {
    const order = this.selectedOrder();
    if (order) {
      this.state.start(order.orderId);
    }
  }

  protected complete(): void {
    const order = this.selectedOrder();
    const kilometers = this.kilometersInput()?.nativeElement.valueAsNumber;
    const performedOn = this.performedOnInput()?.nativeElement.value;
    if (order && kilometers !== undefined && performedOn !== undefined) {
      this.state.complete(order.orderId, kilometers, performedOn);
    }
  }
}
