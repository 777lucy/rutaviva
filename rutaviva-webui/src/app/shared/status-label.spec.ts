import { routineSeverity, statusLabel } from './status-label';

describe('statusLabel', () => {
  it('labels routine statuses in Spanish', () => {
    expect(statusLabel('UpToDate')).toBe('Al día');
    expect(statusLabel('DueSoon')).toBe('Por vencer');
    expect(statusLabel('Overdue')).toBe('Vencida');
  });

  it('labels every service order status in Spanish', () => {
    expect(statusLabel('Created')).toBe('Creada');
    expect(statusLabel('Quoted')).toBe('Cotizada');
    expect(statusLabel('Approved')).toBe('Aprobada');
    expect(statusLabel('InProgress')).toBe('En ejecución');
    expect(statusLabel('Completed')).toBe('Completada');
    expect(statusLabel('Cancelled')).toBe('Cancelada');
  });
});

describe('routineSeverity', () => {
  it('shows up-to-date routines as ok, due-soon as warning, and overdue as danger', () => {
    expect(routineSeverity('UpToDate')).toBe('ok');
    expect(routineSeverity('DueSoon')).toBe('warning');
    expect(routineSeverity('Overdue')).toBe('danger');
  });
});
