import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FleetVehicle } from '../../core/api/models/fleet.model';
import { StatusChipComponent } from '../../shared/components/status-chip/status-chip.component';
import { formatKilometers } from '../../shared/format-thousands';
import { OpenOrderItem } from '../../shared/open-orders';
import { routineSeverity, statusLabel } from '../../shared/status-label';
import { worstRoutineSeverity } from './fleet-summary';

@Component({
  selector: 'app-vehicle-row',
  standalone: true,
  imports: [RouterLink, StatusChipComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './vehicle-row.component.html',
  styleUrl: './vehicle-row.component.scss'
})
export class VehicleRowComponent {
  readonly vehicle = input.required<FleetVehicle>();
  readonly openOrders = input<OpenOrderItem[]>([]);

  protected readonly rowClass = computed(() => `vehicle-row vehicle-row--${worstRoutineSeverity(this.vehicle())}`);
  protected readonly odometerLabel = computed(() => `Odómetro ${formatKilometers(this.vehicle().odometer)}`);

  protected readonly routineChips = computed(() =>
    this.vehicle().routines.map((routine) => ({
      id: routine.id,
      label: `${routine.name} · ${statusLabel(routine.status)}`,
      severity: routineSeverity(routine.status)
    }))
  );

  protected readonly orderLines = computed(() =>
    this.openOrders().map((order) => ({
      id: order.orderId,
      text: `Orden abierta · ${order.routineName} · ${statusLabel(order.status)}`
    }))
  );
}
