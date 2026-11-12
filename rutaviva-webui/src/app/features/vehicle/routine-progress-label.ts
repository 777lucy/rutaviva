import { RoutineBaseline, RoutineRemaining } from '../../core/api/models/vehicle.model';
import { formatKilometers } from '../../shared/format-thousands';

const DATE_FORMAT = new Intl.DateTimeFormat('es', {
  timeZone: 'UTC',
  day: 'numeric',
  month: 'short',
  year: 'numeric'
});

function formatDays(days: number): string {
  return `${days} días`;
}

function remainingPart(amount: number, format: (value: number) => string): string {
  return amount < 0 ? `Vencido ${format(-amount)}` : format(amount);
}

export function routineBaselineLabel(baseline: RoutineBaseline): string {
  const date = DATE_FORMAT.format(new Date(`${baseline.date}T00:00:00Z`));
  return `${formatKilometers(baseline.kilometers)} · ${date}`;
}

export function routineRemainingLabel(remaining: RoutineRemaining): string {
  const parts: string[] = [];
  if (remaining.kilometers !== null) {
    parts.push(remainingPart(remaining.kilometers, formatKilometers));
  }

  if (remaining.days !== null) {
    parts.push(remainingPart(remaining.days, formatDays));
  }

  return parts.join(' · ');
}
