import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { CLIENT_DISPLAY_NAME, CLIENT_INITIALS } from './client-session';
import { SimulationPanelComponent } from './features/simulation/simulation-panel.component';
import { BreadcrumbComponent } from './shared/components/breadcrumb/breadcrumb.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, BreadcrumbComponent, SimulationPanelComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  protected readonly clientName = CLIENT_DISPLAY_NAME;
  protected readonly clientInitials = CLIENT_INITIALS;
}
