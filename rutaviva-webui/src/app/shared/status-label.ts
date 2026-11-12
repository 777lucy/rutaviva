import { RoutineStatus, ServiceOrderStatus } from '../core/api/models/status.model';

export type StatusSeverity = 'ok' | 'warning' | 'danger' | 'info';

const STATUS_LABELS: Record<RoutineStatus | ServiceOrderStatus, string> = {
  UpToDate: 'Al día',
  DueSoon: 'Por vencer',
  Overdue: 'Vencida',
  Created: 'Creada',
  Quoted: 'Cotizada',
  Approved: 'Aprobada',
  InProgress: 'En ejecución',
  Completed: 'Completada',
  Cancelled: 'Cancelada'
};

const ROUTINE_SEVERITY: Record<RoutineStatus, StatusSeverity> = {
  UpToDate: 'ok',
  DueSoon: 'warning',
  Overdue: 'danger'
};

export function statusLabel(status: RoutineStatus | ServiceOrderStatus): string {
  return STATUS_LABELS[status];
}

export function routineSeverity(status: RoutineStatus): StatusSeverity {
  return ROUTINE_SEVERITY[status];
}
