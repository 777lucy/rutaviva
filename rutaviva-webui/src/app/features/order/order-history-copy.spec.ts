import { OrderDetail, OrderHistoryItem } from '../../core/api/models/order.model';
import { orderHistorySteps } from './order-history-copy';

function step(name: string, done: boolean, detail = 'Pendiente'): OrderHistoryItem {
  return { name, detail, done };
}

function orderWith(overrides: Partial<OrderDetail>): OrderDetail {
  return {
    id: 'order-1',
    vehicleId: 'vehicle-1',
    plate: 'XYZ-789',
    routineName: 'Frenos',
    workshopName: 'Taller Central',
    status: 'Quoted',
    quoteAmount: 1800000,
    cancellationReason: null,
    completionKilometers: null,
    completionDate: null,
    history: [
      step('Creada', true, 'La rutina venció'),
      step('Cotizada', true, '1800000 COP'),
      step('Aprobada', false),
      step('En ejecución', false),
      step('Completada', false)
    ],
    ...overrides
  };
}

function details(order: OrderDetail): string[] {
  return orderHistorySteps(order).map((item) => item.detail);
}

describe('orderHistorySteps', () => {
  it('describes a quoted order with the workshop and the formatted quote', () => {
    expect(details(orderWith({}))).toEqual([
      'La rutina venció',
      'Taller Central · 1.800.000 COP',
      'Pendiente de su decisión',
      'El taller aún no inicia',
      'Sin reporte del servicio'
    ]);
  });

  it('waits for the quote on a created order', () => {
    const created = orderWith({
      status: 'Created',
      quoteAmount: null,
      history: [step('Creada', true, 'La rutina venció'), step('Cotizada', false), step('Aprobada', false)]
    });

    expect(details(created)).toEqual(['La rutina venció', 'Esperando la cotización del taller', 'Pendiente']);
  });

  it('describes a completed order with kilometers and date', () => {
    const completed = orderWith({
      status: 'Completed',
      completionKilometers: 102500,
      completionDate: '2026-11-11',
      history: [step('Aprobada', true), step('En ejecución', true), step('Completada', true)]
    });

    expect(details(completed)).toEqual([
      'Cotización aprobada',
      'El taller inició el servicio',
      '102.500 km · 11/11/2026'
    ]);
  });

  it('keeps the done flag and name of each step', () => {
    const [first, second, third] = orderHistorySteps(orderWith({}));

    expect([first.name, first.done]).toEqual(['Creada', true]);
    expect([second.name, second.done]).toEqual(['Cotizada', true]);
    expect([third.name, third.done]).toEqual(['Aprobada', false]);
  });
});
