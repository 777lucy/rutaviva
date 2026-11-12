import {
  ChangeDetectionStrategy,
  Component,
  OnChanges,
  computed,
  effect,
  inject,
  input,
  signal,
  viewChild
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { ACTION_LABELS, actionDialogCopy } from './order-action-copy';
import { ClientAction, clientActions } from './order-actions';
import { OrderStateService } from './order-state.service';
import { OrderTimelineComponent } from './order-timeline.component';
import { cancellationLabel } from './cancellation-label';
import { ACTIONS_NOTE, APPROVAL_ALERT_LINES, orderStatusCopy } from './order-status-copy';
import { BreadcrumbService } from '../../shared/components/breadcrumb/breadcrumb.service';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { StatusChipComponent } from '../../shared/components/status-chip/status-chip.component';
import { formatThousands } from '../../shared/format-thousands';
import { statusLabel } from '../../shared/status-label';

const OrderCrumbLabel = 'Orden';

const ACTION_BUTTON_CLASSES: Record<ClientAction, string> = {
  approve: 'button button--primary order-page__action order-page__action--approve',
  reject: 'button button--danger order-page__action order-page__action--reject',
  cancel: 'button order-page__action order-page__action--cancel'
};

@Component({
  selector: 'app-order-page',
  standalone: true,
  imports: [RouterLink, StatusChipComponent, ConfirmDialogComponent, OrderTimelineComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [OrderStateService],
  templateUrl: './order-page.component.html',
  styleUrl: './order-page.component.scss'
})
export class OrderPageComponent implements OnChanges {
  readonly id = input.required<string>();

  protected readonly order = inject(OrderStateService);
  private readonly breadcrumb = inject(BreadcrumbService);
  private readonly confirmDialog = viewChild.required(ConfirmDialogComponent);
  private readonly pendingAction = signal<ClientAction | null>(null);

  protected readonly approvalAlert = APPROVAL_ALERT_LINES;
  protected readonly actionsNote = ACTIONS_NOTE;

  protected readonly statusChipLabel = computed(() => {
    const detail = this.order.detail();
    return detail ? statusLabel(detail.status) : '';
  });

  protected readonly notice = computed(() => {
    const detail = this.order.detail();
    if (!detail) {
      return '';
    }
    return detail.cancellationReason ? cancellationLabel(detail.cancellationReason) : orderStatusCopy(detail.status);
  });

  protected readonly amountLabel = computed(() => {
    const amount = this.order.detail()?.quoteAmount;
    return amount == null ? '' : `$ ${formatThousands(amount)}`;
  });

  protected readonly actions = computed(() => {
    const detail = this.order.detail();
    return detail
      ? clientActions(detail.status).map((action) => ({
          action,
          label: ACTION_LABELS[action],
          classes: ACTION_BUTTON_CLASSES[action]
        }))
      : [];
  });

  protected readonly dialogCopy = computed(() => {
    const detail = this.order.detail();
    return detail ? actionDialogCopy(this.pendingAction() ?? 'approve', detail) : null;
  });

  constructor() {
    effect(
      () => {
        const detail = this.order.detail();
        this.breadcrumb.show(
          detail
            ? [
                { label: detail.plate, link: ['/vehicles', detail.vehicleId] },
                { label: OrderCrumbLabel, link: null }
              ]
            : []
        );
      },
      { allowSignalWrites: true }
    );
  }

  ngOnChanges(): void {
    this.order.load(this.id());
  }

  protected askConfirmation(action: ClientAction): void {
    this.pendingAction.set(action);
    this.confirmDialog().open();
  }

  protected runPendingAction(): void {
    const action = this.pendingAction();
    if (action) {
      this.order.runAction(this.id(), action);
    }
  }
}
