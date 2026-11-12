const BOGOTA_TIME_ZONE = 'America/Bogota';

const BOGOTA_DATE_FORMAT = new Intl.DateTimeFormat('en-CA', {
  timeZone: BOGOTA_TIME_ZONE,
  year: 'numeric',
  month: '2-digit',
  day: '2-digit'
});

export function todayInBogota(now: Date): string {
  return BOGOTA_DATE_FORMAT.format(now);
}
