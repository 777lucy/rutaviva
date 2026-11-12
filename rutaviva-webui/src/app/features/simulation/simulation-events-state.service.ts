import { Injectable, computed, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { SystemApiService } from '../../core/api/system-api.service';
import { TelemetryApiService } from '../../core/api/telemetry-api.service';
import { RecordedEvent } from '../../core/api/models/recorded-event.model';
import { ReadingResult } from '../../core/api/models/telemetry.model';
import { problemTitle } from '../../core/problem-title';
import { RefreshService, reloadOnRefresh } from '../../core/refresh.service';
import { ignoredReadingEvent } from './simulation-copy';

function newestFirst(first: RecordedEvent, second: RecordedEvent): number {
  return second.occurredAt.localeCompare(first.occurredAt);
}

@Injectable()
export class SimulationEventsStateService {
  private readonly telemetryApi = inject(TelemetryApiService);
  private readonly systemApi = inject(SystemApiService);
  private readonly refresh = inject(RefreshService);

  private readonly recordedEvents = signal<RecordedEvent[]>([]);
  // Las lecturas ignoradas no generan evento en la API; se muestran solo en esta sesión.
  private readonly ignoredReadings = signal<RecordedEvent[]>([]);

  readonly events = computed(() => [...this.ignoredReadings(), ...this.recordedEvents()].sort(newestFirst));
  readonly readingBusy = signal(false);
  readonly reviewBusy = signal(false);
  readonly commandError = signal<string | null>(null);

  constructor() {
    reloadOnRefresh(this.refresh, () => this.loadEvents());
  }

  loadEvents(): void {
    this.systemApi.getEvents().subscribe({
      next: (events) => this.recordedEvents.set(events),
      error: (error: unknown) => this.commandError.set(problemTitle(error))
    });
  }

  submitReading(vehicleId: string, kilometers: number): void {
    this.readingBusy.set(true);
    this.commandError.set(null);
    this.telemetryApi
      .registerReading(vehicleId, kilometers)
      .pipe(finalize(() => this.readingBusy.set(false)))
      .subscribe({
        next: (outcome) => this.handleReadingResult(vehicleId, outcome.result),
        error: (error: unknown) => this.commandError.set(problemTitle(error))
      });
  }

  runDailyReview(date: string): void {
    this.reviewBusy.set(true);
    this.commandError.set(null);
    this.systemApi
      .runDailyReview(date)
      .pipe(finalize(() => this.reviewBusy.set(false)))
      .subscribe({
        next: () => this.refresh.notifyCommandSucceeded(),
        error: (error: unknown) => this.commandError.set(problemTitle(error))
      });
  }

  private handleReadingResult(vehicleId: string, result: ReadingResult): void {
    if (result === 'Accepted') {
      this.refresh.notifyCommandSucceeded();
      return;
    }
    this.ignoredReadings.update((events) => [ignoredReadingEvent(vehicleId, new Date()), ...events]);
  }
}
