import { RecordedEvent } from '../../../core/api/models/recorded-event.model';
import { VehicleDetail } from '../../../core/api/models/vehicle.model';
import { eventLine } from './event-line';

const vehicles: VehicleDetail[] = [
  {
    id: 'vehicle-1',
    plate: 'XYZ-789',
    description: 'Camión 5 t',
    odometer: 102500,
    workshopName: 'Taller Central',
    routines: [
      {
        id: 'routine-brakes',
        name: 'Frenos',
        interval: 'cada 20.000 km',
        status: 'Overdue',
        openOrder: { id: 'order-1', status: 'Quoted' },
        baseline: { kilometers: 82500, date: '2026-01-01' },
        remaining: { kilometers: -2500, days: null }
      }
    ]
  }
];

function event(overrides: Partial<RecordedEvent>): RecordedEvent {
  return {
    name: 'QuoteSubmitted',
    title: 'Cotización enviada',
    vehicleId: null,
    routineId: null,
    orderId: null,
    occurredAt: '2026-11-11T14:00:00Z',
    ...overrides
  };
}

describe('eventLine', () => {
  it('names the plate and routine from the vehicle and routine ids', () => {
    expect(eventLine(event({ vehicleId: 'vehicle-1', routineId: 'routine-brakes' }), vehicles)).toBe(
      'Cotización enviada · XYZ-789 Frenos'
    );
  });

  it('resolves the plate and routine of an open order when the event only carries the order id', () => {
    expect(eventLine(event({ orderId: 'order-1' }), vehicles)).toBe('Cotización enviada · XYZ-789 Frenos');
  });

  it('shows only the title when nothing can be resolved', () => {
    expect(eventLine(event({ orderId: 'order-gone' }), vehicles)).toBe('Cotización enviada');
  });
});
