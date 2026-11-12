import { CancellationReason } from '../../core/api/models/status.model';

const CANCELLATION_LABELS: Record<CancellationReason, string> = {
  QuoteRejected: 'Cotización rechazada',
  CancelledByClient: 'Cancelada por el cliente'
};

export function cancellationLabel(reason: CancellationReason): string {
  return CANCELLATION_LABELS[reason];
}
