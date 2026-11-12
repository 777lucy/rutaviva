import { todayInBogota } from './bogota-today';

describe('todayInBogota', () => {
  it('returns the calendar date in America/Bogota as yyyy-MM-dd', () => {
    expect(todayInBogota(new Date('2026-11-11T14:00:00Z'))).toBe('2026-11-11');
  });

  it('uses the Bogota date when UTC has already moved to the next day', () => {
    expect(todayInBogota(new Date('2026-11-11T03:00:00Z'))).toBe('2026-11-10');
  });
});
