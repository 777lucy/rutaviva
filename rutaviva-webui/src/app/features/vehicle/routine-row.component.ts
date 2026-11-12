import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { VehicleRoutine } from '../../core/api/models/vehicle.model';
import { StatusChipComponent } from '../../shared/components/status-chip/status-chip.component';
import { routineSeverity, statusLabel } from '../../shared/status-label';
import { routineBaselineLabel, routineRemainingLabel } from './routine-progress-label';

@Component({
  selector: 'app-routine-row',
  standalone: true,
  imports: [RouterLink, StatusChipComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './routine-row.component.html',
  styleUrl: './routine-row.component.scss'
})
export class RoutineRowComponent {
  readonly routine = input.required<VehicleRoutine>();

  protected readonly chip = computed(() => ({
    label: statusLabel(this.routine().status),
    severity: routineSeverity(this.routine().status)
  }));

  protected readonly baseline = computed(() => routineBaselineLabel(this.routine().baseline));

  protected readonly remaining = computed(() => routineRemainingLabel(this.routine().remaining));
}
