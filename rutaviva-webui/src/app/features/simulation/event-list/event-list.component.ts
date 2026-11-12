import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { SimulationEventsStateService } from '../simulation-events-state.service';
import { SimulationStateService } from '../simulation-state.service';
import { eventLine } from './event-line';

const MaxVisibleEvents = 8;

@Component({
  selector: 'app-event-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './event-list.component.html',
  styleUrl: './event-list.component.scss'
})
export class EventListComponent {
  private readonly eventsState = inject(SimulationEventsStateService);
  private readonly simulation = inject(SimulationStateService);

  protected readonly recentLines = computed(() => {
    const details = this.simulation.details();
    return this.eventsState
      .events()
      .slice(0, MaxVisibleEvents)
      .map((event) => eventLine(event, details));
  });
}
