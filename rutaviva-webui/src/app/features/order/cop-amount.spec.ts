import { formatCop } from './cop-amount';

describe('formatCop', () => {
  it('groups thousands with dots and appends COP', () => {
    expect(formatCop(1500000)).toBe('1.500.000 COP');
    expect(formatCop(950000)).toBe('950.000 COP');
  });

  it('formats amounts under one thousand without separators', () => {
    expect(formatCop(0)).toBe('0 COP');
    expect(formatCop(850)).toBe('850 COP');
  });
});
