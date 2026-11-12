import { ChangeDetectionStrategy, Component, OnInit, computed, inject } from '@angular/core';
import { OpenOrderItem } from '../../shared/open-orders';
import { BreadcrumbService } from '../../shared/components/breadcrumb/breadcrumb.service';
import { StatusChipComponent } from '../../shared/components/status-chip/status-chip.component';
import { DueSoonBannerComponent } from './due-soon-banner.component';
import { FleetStateService } from './fleet-state.service';
import { FleetStatsComponent } from './fleet-stats.component';
import { fleetSummary } from './fleet-summary';
import { VehicleRowComponent } from './vehicle-row.component';

@Component({
  selector: 'app-fleet-page',
  standalone: true,
  imports: [DueSoonBannerComponent, FleetStatsComponent, StatusChipComponent, VehicleRowComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './fleet-page.component.html',
  styleUrl: './fleet-page.component.scss'
})
export class FleetPageComponent implements OnInit {
  protected readonly fleet = inject(FleetStateService);
  private readonly breadcrumb = inject(BreadcrumbService);

  protected readonly summary = computed(() => fleetSummary(this.fleet.vehicles(), this.fleet.openOrders()));

  ngOnInit(): void {
    this.breadcrumb.show([]);
    this.fleet.load();
  }

  protected openOrdersOf(vehicleId: string): OpenOrderItem[] {
    return this.fleet.openOrders().filter((order) => order.vehicleId === vehicleId);
  }
}
