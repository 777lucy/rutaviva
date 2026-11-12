import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { todayInBogota } from '../bogota-today';
import { SimulationEventsStateService } from '../simulation-events-state.service';

@Component({
  selector: 'app-daily-review-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './daily-review-form.component.html',
  styleUrl: './daily-review-form.component.scss'
})
export class DailyReviewFormComponent {
  protected readonly events = inject(SimulationEventsStateService);
  protected readonly today = todayInBogota(new Date());

  protected submit(date: string): void {
    this.events.runDailyReview(date);
  }
}
