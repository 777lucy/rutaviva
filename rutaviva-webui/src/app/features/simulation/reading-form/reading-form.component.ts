import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { SimulationEventsStateService } from '../simulation-events-state.service';
import { SimulationStateService } from '../simulation-state.service';

@Component({
  selector: 'app-reading-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './reading-form.component.html',
  styleUrl: './reading-form.component.scss'
})
export class ReadingFormComponent {
  protected readonly orders = inject(SimulationStateService);
  protected readonly events = inject(SimulationEventsStateService);

  protected submit(vehicleId: string, kilometers: number): void {
    this.events.submitReading(vehicleId, kilometers);
  }
}
