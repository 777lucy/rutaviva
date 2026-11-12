const THOUSANDS_SEPARATOR_PATTERN = /\B(?=(\d{3})+(?!\d))/g;

export function formatThousands(value: number): string {
  return Math.round(value).toString().replace(THOUSANDS_SEPARATOR_PATTERN, '.');
}

export function formatKilometers(kilometers: number): string {
  return `${formatThousands(kilometers)} km`;
}
