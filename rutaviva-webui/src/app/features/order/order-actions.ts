import { ServiceOrderStatus } from '../../core/api/models/status.model';

export type ClientAction = 'approve' | 'reject' | 'cancel';

const CLIENT_ACTIONS: Partial<Record<ServiceOrderStatus, ClientAction[]>> = {
  Created: ['cancel'],
  Quoted: ['approve', 'reject', 'cancel'],
  Approved: ['cancel']
};

export function clientActions(status: ServiceOrderStatus): ClientAction[] {
  return CLIENT_ACTIONS[status] ?? [];
}
