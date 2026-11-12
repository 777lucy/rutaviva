import { ServiceOrderStatus } from '../core/api/models/status.model';
import { VehicleDetail } from '../core/api/models/vehicle.model';

export interface OpenOrderItem {
  orderId: string;
  vehicleId: string;
  plate: string;
  routineName: string;
  status: ServiceOrderStatus;
}

export function openOrdersOf(vehicles: VehicleDetail[]): OpenOrderItem[] {
  return vehicles.flatMap((vehicle) =>
    vehicle.routines.flatMap((routine) => {
      const openOrder = routine.openOrder;
      return openOrder ? [{ orderId: openOrder.id, vehicleId: vehicle.id, plate: vehicle.plate, routineName: routine.name, status: openOrder.status }] : [];
    })
  );
}
