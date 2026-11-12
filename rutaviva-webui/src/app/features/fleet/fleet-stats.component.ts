import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { FleetSummary } from './fleet-summary';

@Component({
  selector: 'app-fleet-stats',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './fleet-stats.component.html',
  styleUrl: './fleet-stats.component.scss'
})
export class FleetStatsComponent {
  readonly summary = input.required<FleetSummary>();

  protected readonly tiles = computed(() => {
    const summary = this.summary();
    return [
      { label: 'Vehículos', value: summary.vehicles, tone: 'ink' },
      { label: 'Por vencer', value: summary.dueSoon, tone: 'warning' },
      { label: 'Vencida', value: summary.overdue, tone: 'danger' },
      { label: 'Orden por cotizar', value: summary.awaitingQuote, tone: 'primary' }
    ];
  });
}
