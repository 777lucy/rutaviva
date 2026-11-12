import { ServiceOrderStatus } from '../../core/api/models/status.model';

export const APPROVAL_ALERT_LINES = {
  title: 'Requiere su aprobación porque supera 1.500.000 COP.',
  detail: 'Un valor igual o menor se aprueba sin pasar por usted.'
};

export const ACTIONS_NOTE =
  'Rechazar cancela la orden. Cancelar deja de estar disponible cuando el taller ya inició el servicio.';

const STATUS_COPY: Partial<Record<ServiceOrderStatus, string>> = {
  Created: 'Esperando la cotización del taller',
  Approved: 'Aprobada',
  InProgress: 'El taller ya inició el servicio',
  Completed: 'La rutina volvió a estar al día'
};

export function orderStatusCopy(status: ServiceOrderStatus): string {
  return STATUS_COPY[status] ?? '';
}
