import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AppComponent } from './app.component';
import { appConfig } from './app.config';

describe('AppComponent shell (Smoke)', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [...appConfig.providers, provideHttpClientTesting()]
    }).compileComponents();
  });

  it('shows the RutaViva brand, the breadcrumb, and the seeded client pill in the header', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const header: HTMLElement = fixture.nativeElement.querySelector('header');

    expect(header.querySelector('.app-header__logo')?.textContent?.trim()).toBe('R');
    expect(header.querySelector('.app-header__brand')?.textContent?.trim()).toBe('RutaViva');
    expect(header.querySelector('.breadcrumb')?.textContent).toContain('Flota');
    expect(header.querySelector('.app-header__avatar')?.textContent?.trim()).toBe('TA');
    expect(header.querySelector('.app-header__client')?.textContent?.trim()).toBe('Transportes Andinos');
  });

  it('renders the router outlet beside the simulation panel', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const element: HTMLElement = fixture.nativeElement;

    expect(element.querySelector('router-outlet')).not.toBeNull();
    expect(element.querySelector('aside h2')?.textContent).toContain('Simulación');
  });

  it('mounts the real simulation panel in the shell aside', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const element: HTMLElement = fixture.nativeElement;

    expect(element.querySelector('aside app-simulation-panel')).not.toBeNull();
  });
});
