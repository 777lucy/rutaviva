import { FleetVehicle } from '../../core/api/models/fleet.model';
import { OpenOrderItem } from '../../shared/open-orders';
import { fleetSummary, worstRoutineSeverity } from './fleet-summary';

const fleet: FleetVehicle[] = [
  {
    id: 'vehicle-abc',
    plate: 'ABC-123',
    description: 'Camión 3 t',
    odometer: 59000,
    awaitingQuote: false,
    routines: [
      { id: 'oil', name: 'Aceite', status: 'DueSoon' },
      { id: 'tyres', name: 'Llantas', status: 'UpToDate' }
    ]
  },
  {
    id: 'vehicle-xyz',
    plate: 'XYZ-789',
    description: 'Camión 5 t',
    odometer: 102100,
    awaitingQuote: false,
    routines: [{ id: 'brakes', name: 'Frenos', status: 'Overdue' }]
  }
];

function openOrder(status: OpenOrderItem['status']): OpenOrderItem {
  return { orderId: `order-${status}`, vehicleId: 'vehicle-xyz', plate: 'XYZ-789', routineName: 'Frenos', status };
}

describe('fleetSummary', () => {
  it('counts vehicles, due-soon and overdue routines, and orders waiting for the workshop quote', () => {
    const summary = fleetSummary(fleet, [openOrder('Created'), openOrder('Quoted')]);

    expect(summary).toEqual({ vehicles: 2, dueSoon: 1, overdue: 1, awaitingQuote: 1 });
  });
});

describe('worstRoutineSeverity', () => {
  it('uses the most urgent routine status of the vehicle', () => {
    expect(worstRoutineSeverity(fleet[0])).toBe('warning');
    expect(worstRoutineSeverity(fleet[1])).toBe('danger');
    expect(worstRoutineSeverity({ ...fleet[0], routines: [] })).toBe('ok');
  });
});
