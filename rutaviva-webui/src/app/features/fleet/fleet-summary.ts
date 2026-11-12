import { FleetVehicle } from '../../core/api/models/fleet.model';
import { RoutineStatus } from '../../core/api/models/status.model';
import { OpenOrderItem } from '../../shared/open-orders';
import { StatusSeverity, routineSeverity } from '../../shared/status-label';

export interface FleetSummary {
  vehicles: number;
  dueSoon: number;
  overdue: number;
  awaitingQuote: number;
}

const STATUS_URGENCY: RoutineStatus[] = ['Overdue', 'DueSoon', 'UpToDate'];

export function fleetSummary(fleet: FleetVehicle[], openOrders: OpenOrderItem[]): FleetSummary {
  const statuses = fleet.flatMap((vehicle) => vehicle.routines.map((routine) => routine.status));
  return {
    vehicles: fleet.length,
    dueSoon: statuses.filter((status) => status === 'DueSoon').length,
    overdue: statuses.filter((status) => status === 'Overdue').length,
    awaitingQuote: openOrders.filter((order) => order.status === 'Created').length
  };
}

export function worstRoutineSeverity(vehicle: FleetVehicle): StatusSeverity {
  const statuses = vehicle.routines.map((routine) => routine.status);
  const worst = STATUS_URGENCY.find((status) => statuses.includes(status)) ?? 'UpToDate';
  return routineSeverity(worst);
}
