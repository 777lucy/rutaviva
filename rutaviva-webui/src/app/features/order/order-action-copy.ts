import { OrderDetail } from '../../core/api/models/order.model';
import { ConfirmTone } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { formatCop } from './cop-amount';
import { ClientAction } from './order-actions';

export interface ActionDialogCopy {
  title: string;
  message: string;
  detail: string;
  confirmLabel: string;
  tone: ConfirmTone;
}

export const ACTION_LABELS: Record<ClientAction, string> = {
  approve: 'Aprobar',
  reject: 'Rechazar',
  cancel: 'Cancelar'
};

const CancelledDetail = 'La rutina sigue vencida. Esta versión no abre otra orden.';

function orderSubject(order: OrderDetail): string {
  return `La orden de ${order.routineName} para ${order.plate}`;
}

const DIALOG_COPY: Record<ClientAction, (order: OrderDetail) => ActionDialogCopy> = {
  approve: (order) => ({
    title: '¿Aprobar la cotización?',
    message: `${orderSubject(order)} quedará aprobada por ${formatCop(order.quoteAmount ?? 0)}.`,
    detail: 'El taller podrá iniciar el servicio.',
    confirmLabel: 'Sí, aprobar',
    tone: 'primary'
  }),
  reject: (order) => ({
    title: '¿Rechazar la cotización?',
    message: `${orderSubject(order)} quedará cancelada.`,
    detail: CancelledDetail,
    confirmLabel: 'Sí, rechazar',
    tone: 'danger'
  }),
  cancel: (order) => ({
    title: '¿Cancelar la orden?',
    message: `${orderSubject(order)} quedará cancelada.`,
    detail: CancelledDetail,
    confirmLabel: 'Sí, cancelar',
    tone: 'danger'
  })
};

export function actionDialogCopy(action: ClientAction, order: OrderDetail): ActionDialogCopy {
  return DIALOG_COPY[action](order);
}
