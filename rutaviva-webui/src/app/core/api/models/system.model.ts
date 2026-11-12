import { RoutineStatus } from './status.model';

export interface RoutineChange {
  vehicleId: string;
  routineId: string;
  status: RoutineStatus;
}
