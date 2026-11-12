import { RoutineStatus } from './status.model';

export interface FleetRoutine {
  id: string;
  name: string;
  status: RoutineStatus;
}

export interface FleetVehicle {
  id: string;
  plate: string;
  description: string;
  odometer: number;
  awaitingQuote: boolean;
  routines: FleetRoutine[];
}
