import { CancellationReason, ServiceOrderStatus } from './status.model';

export interface OrderHistoryItem {
  name: string;
  detail: string;
  done: boolean;
}

export interface OrderDetail {
  id: string;
  vehicleId: string;
  plate: string;
  routineName: string;
  workshopName: string;
  status: ServiceOrderStatus;
  quoteAmount: number | null;
  cancellationReason: CancellationReason | null;
  completionKilometers: number | null;
  completionDate: string | null;
  history: OrderHistoryItem[];
}
