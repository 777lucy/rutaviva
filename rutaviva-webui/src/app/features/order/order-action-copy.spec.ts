import { OrderDetail } from '../../core/api/models/order.model';
import { actionDialogCopy } from './order-action-copy';

const order: OrderDetail = {
  id: 'order-1',
  vehicleId: 'vehicle-1',
  plate: 'XYZ-789',
  routineName: 'Frenos',
  workshopName: 'Taller Central',
  status: 'Quoted',
  quoteAmount: 1800000,
  cancellationReason: null,
  completionKilometers: null,
  completionDate: null,
  history: []
};

describe('actionDialogCopy', () => {
  it('asks to reject the quote and explains the routine stays overdue', () => {
    expect(actionDialogCopy('reject', order)).toEqual({
      title: '¿Rechazar la cotización?',
      message: 'La orden de Frenos para XYZ-789 quedará cancelada.',
      detail: 'La rutina sigue vencida. Esta versión no abre otra orden.',
      confirmLabel: 'Sí, rechazar',
      tone: 'danger'
    });
  });

  it('asks to approve the quote with its formatted amount', () => {
    expect(actionDialogCopy('approve', order)).toEqual({
      title: '¿Aprobar la cotización?',
      message: 'La orden de Frenos para XYZ-789 quedará aprobada por 1.800.000 COP.',
      detail: 'El taller podrá iniciar el servicio.',
      confirmLabel: 'Sí, aprobar',
      tone: 'primary'
    });
  });

  it('asks to cancel the order', () => {
    expect(actionDialogCopy('cancel', order)).toEqual({
      title: '¿Cancelar la orden?',
      message: 'La orden de Frenos para XYZ-789 quedará cancelada.',
      detail: 'La rutina sigue vencida. Esta versión no abre otra orden.',
      confirmLabel: 'Sí, cancelar',
      tone: 'danger'
    });
  });
});
