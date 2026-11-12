import { ChangeDetectionStrategy, Component, OnChanges, computed, effect, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BreadcrumbService } from '../../shared/components/breadcrumb/breadcrumb.service';
import { formatKilometers } from '../../shared/format-thousands';
import { RoutineRowComponent } from './routine-row.component';
import { VehicleStateService } from './vehicle-state.service';

@Component({
  selector: 'app-vehicle-page',
  standalone: true,
  imports: [RouterLink, RoutineRowComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [VehicleStateService],
  templateUrl: './vehicle-page.component.html',
  styleUrl: './vehicle-page.component.scss'
})
export class VehiclePageComponent implements OnChanges {
  readonly id = input.required<string>();

  protected readonly vehicle = inject(VehicleStateService);
  private readonly breadcrumb = inject(BreadcrumbService);

  protected readonly odometerLabel = computed(() => {
    const detail = this.vehicle.detail();
    return detail ? `Odómetro ${formatKilometers(detail.odometer)}` : '';
  });

  constructor() {
    effect(
      () => {
        const detail = this.vehicle.detail();
        this.breadcrumb.show(detail ? [{ label: detail.plate, link: null }] : []);
      },
      { allowSignalWrites: true }
    );
  }

  ngOnChanges(): void {
    this.vehicle.load(this.id());
  }
}
