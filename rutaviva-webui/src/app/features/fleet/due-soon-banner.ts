import { FleetVehicle } from '../../core/api/models/fleet.model';
import { RecordedEvent } from '../../core/api/models/recorded-event.model';

export interface DueSoonBanner {
  vehicleId: string;
  routineId: string;
  text: string;
}

const DUE_SOON_EVENT = 'RoutineBecameDueSoon';
const CYCLE_RESTART_EVENT = 'ServiceCompleted';

export function activeDueSoonBanners(eventsNewestFirst: RecordedEvent[], fleet: FleetVehicle[]): DueSoonBanner[] {
  const activeKeys = activeDueSoonKeys(eventsNewestFirst);

  return fleet.flatMap((vehicle) =>
    vehicle.routines
      .filter((routine) => activeKeys.has(cycleKey(vehicle.id, routine.id)))
      .map((routine) => ({
        vehicleId: vehicle.id,
        routineId: routine.id,
        text: `${vehicle.plate} · ${routine.name} está por vencer.`
      }))
  );
}

// El evento más reciente de cada rutina decide si el aviso sigue activo.
function activeDueSoonKeys(eventsNewestFirst: RecordedEvent[]): Set<string> {
  const decided = new Set<string>();
  const active = new Set<string>();

  for (const event of eventsNewestFirst) {
    const key = eventCycleKey(event);
    if (key === null || decided.has(key) || !isCycleEvent(event.name)) {
      continue;
    }

    decided.add(key);
    if (event.name === DUE_SOON_EVENT) {
      active.add(key);
    }
  }

  return active;
}

function isCycleEvent(name: string): boolean {
  return name === DUE_SOON_EVENT || name === CYCLE_RESTART_EVENT;
}

function eventCycleKey(event: RecordedEvent): string | null {
  return event.vehicleId && event.routineId ? cycleKey(event.vehicleId, event.routineId) : null;
}

function cycleKey(vehicleId: string, routineId: string): string {
  return `${vehicleId}/${routineId}`;
}
