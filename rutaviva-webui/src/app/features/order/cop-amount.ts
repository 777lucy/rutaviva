import { formatThousands } from '../../shared/format-thousands';

export function formatCop(amount: number): string {
  return `${formatThousands(amount)} COP`;
}
