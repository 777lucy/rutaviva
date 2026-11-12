import { ServiceOrderStatus } from '../../core/api/models/status.model';

export type WorkshopAction = 'quote' | 'start' | 'complete';

const WORKSHOP_ACTIONS: Partial<Record<ServiceOrderStatus, WorkshopAction[]>> = {
  Created: ['quote'],
  Approved: ['start'],
  InProgress: ['complete']
};

export function workshopActions(status: ServiceOrderStatus): WorkshopAction[] {
  return WORKSHOP_ACTIONS[status] ?? [];
}
