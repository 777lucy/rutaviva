import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-due-soon-banner',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './due-soon-banner.component.html',
  styleUrl: './due-soon-banner.component.scss'
})
export class DueSoonBannerComponent {
  readonly text = input.required<string>();
}
