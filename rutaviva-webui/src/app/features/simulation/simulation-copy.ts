import { RecordedEvent } from '../../core/api/models/recorded-event.model';
import { WorkshopAction } from './workshop-actions';

export const WORKSHOP_ACTION_LABELS: Record<WorkshopAction, string> = {
  quote: 'Enviar cotización',
  start: 'Iniciar',
  complete: 'Completar'
};

export const DefaultWorkshopTitle = 'Taller';

export function ignoredReadingEvent(vehicleId: string, now: Date): RecordedEvent {
  return {
    name: 'ReadingIgnored',
    title: 'Lectura ignorada',
    vehicleId,
    routineId: null,
    orderId: null,
    occurredAt: now.toISOString()
  };
}
