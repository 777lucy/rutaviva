import { routineBaselineLabel, routineRemainingLabel } from './routine-progress-label';

describe('routine progress labels', () => {
  it('shows the baseline kilometers and its date in Spanish', () => {
    expect(routineBaselineLabel({ kilometers: 50000, date: '2026-01-01' })).toBe('50.000 km · 1 ene 2026');
  });

  it('shows the remaining kilometers and days separated by a dot', () => {
    expect(routineRemainingLabel({ kilometers: 3700, days: 82 })).toBe('3.700 km · 82 días');
  });

  it('shows only the dimension the routine has', () => {
    expect(routineRemainingLabel({ kilometers: null, days: 82 })).toBe('82 días');
  });

  it('marks an exceeded dimension as overdue by how much', () => {
    expect(routineRemainingLabel({ kilometers: -1000, days: -10 })).toBe('Vencido 1.000 km · Vencido 10 días');
  });
});
