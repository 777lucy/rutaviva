import { FleetVehicle } from '../../core/api/models/fleet.model';
import { RecordedEvent } from '../../core/api/models/recorded-event.model';
import { activeDueSoonBanners } from './due-soon-banner';

const VEHICLE_ID = 'vehicle-abc';
const ROUTINE_ID = 'routine-oil';

const fleet: FleetVehicle[] = [
  {
    id: VEHICLE_ID,
    plate: 'ABC-123',
    description: 'Camión 3 ejes',
    odometer: 59000,
    awaitingQuote: false,
    routines: [{ id: ROUTINE_ID, name: 'Aceite', status: 'DueSoon' }]
  }
];

function routineEvent(name: string): RecordedEvent {
  return {
    name,
    title: name,
    vehicleId: VEHICLE_ID,
    routineId: ROUTINE_ID,
    orderId: null,
    occurredAt: '2026-11-11T13:00:00Z'
  };
}

describe('activeDueSoonBanners', () => {
  it('shows "{plate} · {routine} está por vencer." after the routine became due soon', () => {
    const banners = activeDueSoonBanners([routineEvent('RoutineBecameDueSoon')], fleet);

    expect(banners.map((banner) => banner.text)).toEqual(['ABC-123 · Aceite está por vencer.']);
  });

  it('removes the banner once a later service completion restarts the routine', () => {
    const events = [routineEvent('ServiceCompleted'), routineEvent('RoutineBecameDueSoon')];

    expect(activeDueSoonBanners(events, fleet)).toEqual([]);
  });

  it('keeps the banner after the routine later becomes overdue', () => {
    const events = [routineEvent('RoutineBecameOverdue'), routineEvent('RoutineBecameDueSoon')];

    expect(activeDueSoonBanners(events, fleet).length).toBe(1);
  });

  it('shows no banner when a routine jumped straight to overdue', () => {
    expect(activeDueSoonBanners([routineEvent('RoutineBecameOverdue')], fleet)).toEqual([]);
  });
});
