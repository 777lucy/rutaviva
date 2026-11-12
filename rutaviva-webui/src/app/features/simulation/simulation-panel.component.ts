import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { DailyReviewFormComponent } from './daily-review-form/daily-review-form.component';
import { EventListComponent } from './event-list/event-list.component';
import { ReadingFormComponent } from './reading-form/reading-form.component';
import { SimulationEventsStateService } from './simulation-events-state.service';
import { SimulationStateService } from './simulation-state.service';
import { WorkshopSectionComponent } from './workshop-section/workshop-section.component';

@Component({
  selector: 'app-simulation-panel',
  standalone: true,
  imports: [WorkshopSectionComponent, ReadingFormComponent, DailyReviewFormComponent, EventListComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [SimulationStateService, SimulationEventsStateService],
  templateUrl: './simulation-panel.component.html',
  styleUrl: './simulation-panel.component.scss'
})
export class SimulationPanelComponent implements OnInit {
  protected readonly events = inject(SimulationEventsStateService);
  private readonly orders = inject(SimulationStateService);

  ngOnInit(): void {
    this.orders.load();
    this.events.loadEvents();
  }
}
