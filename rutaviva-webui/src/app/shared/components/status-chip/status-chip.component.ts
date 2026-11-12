import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { StatusSeverity } from '../../status-label';

@Component({
  selector: 'app-status-chip',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './status-chip.component.html',
  styleUrl: './status-chip.component.scss'
})
export class StatusChipComponent {
  readonly label = input.required<string>();
  readonly severity = input.required<StatusSeverity>();

  protected readonly chipClass = computed(() => `status-chip status-chip--${this.severity()}`);
}
