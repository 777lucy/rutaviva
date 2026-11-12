import { OrderDetail, OrderHistoryItem } from '../../core/api/models/order.model';
import { formatKilometers } from '../../shared/format-thousands';
import { formatCop } from './cop-amount';

type StepCopy = (order: OrderDetail, item: OrderHistoryItem) => string;

const STEP_COPY: Record<string, StepCopy> = {
  Cotizada: (order, item) =>
    item.done && order.quoteAmount !== null
      ? `${order.workshopName} · ${formatCop(order.quoteAmount)}`
      : 'Esperando la cotización del taller',
  Aprobada: (order, item) => {
    if (item.done) {
      return 'Cotización aprobada';
    }
    return order.status === 'Quoted' ? 'Pendiente de su decisión' : item.detail;
  },
  'En ejecución': (_order, item) => (item.done ? 'El taller inició el servicio' : 'El taller aún no inicia'),
  Completada: (order, item) => (item.done ? completionCopy(order) : 'Sin reporte del servicio')
};

function completionCopy(order: OrderDetail): string {
  const kilometers = order.completionKilometers === null ? '' : formatKilometers(order.completionKilometers);
  return [kilometers, displayDate(order.completionDate)].filter(Boolean).join(' · ');
}

function displayDate(isoDate: string | null): string {
  if (!isoDate) {
    return '';
  }
  const [year, month, day] = isoDate.slice(0, 10).split('-');
  return `${day}/${month}/${year}`;
}

export function orderHistorySteps(order: OrderDetail): OrderHistoryItem[] {
  return order.history.map((item) => ({
    ...item,
    detail: STEP_COPY[item.name]?.(order, item) ?? item.detail
  }));
}
