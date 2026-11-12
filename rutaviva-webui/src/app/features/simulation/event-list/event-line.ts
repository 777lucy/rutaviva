import { RecordedEvent } from '../../../core/api/models/recorded-event.model';
import { VehicleDetail, VehicleRoutine } from '../../../core/api/models/vehicle.model';

function belongsToEvent(routine: VehicleRoutine, event: RecordedEvent): boolean {
  return routine.id === event.routineId || (event.orderId !== null && routine.openOrder?.id === event.orderId);
}

function vehicleOf(event: RecordedEvent, vehicles: VehicleDetail[]): VehicleDetail | undefined {
  return (
    vehicles.find((vehicle) => vehicle.id === event.vehicleId) ??
    vehicles.find((vehicle) => vehicle.routines.some((routine) => belongsToEvent(routine, event)))
  );
}

export function eventLine(event: RecordedEvent, vehicles: VehicleDetail[]): string {
  const vehicle = vehicleOf(event, vehicles);
  const routine = vehicle?.routines.find((candidate) => belongsToEvent(candidate, event));
  const subject = [vehicle?.plate ?? '', routine?.name ?? ''].filter(Boolean).join(' ');
  return [event.title, subject].filter(Boolean).join(' · ');
}
