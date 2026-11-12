import { RoutineStatus, ServiceOrderStatus } from './status.model';

export interface OpenOrder {
  id: string;
  status: ServiceOrderStatus;
}

export interface RoutineBaseline {
  kilometers: number;
  date: string;
}

export interface RoutineRemaining {
  kilometers: number | null;
  days: number | null;
}

export interface VehicleRoutine {
  id: string;
  name: string;
  interval: string;
  status: RoutineStatus;
  openOrder: OpenOrder | null;
  baseline: RoutineBaseline;
  remaining: RoutineRemaining;
}

export interface VehicleDetail {
  id: string;
  plate: string;
  description: string;
  odometer: number;
  workshopName: string;
  routines: VehicleRoutine[];
}
