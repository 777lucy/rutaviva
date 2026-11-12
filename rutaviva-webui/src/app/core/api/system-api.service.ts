import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiUrl } from './api-url';
import { RecordedEvent } from './models/recorded-event.model';
import { RoutineChange } from './models/system.model';

@Injectable({ providedIn: 'root' })
export class SystemApiService {
  private readonly http = inject(HttpClient);

  runDailyReview(date: string): Observable<RoutineChange[]> {
    return this.http.post<RoutineChange[]>(apiUrl('/api/system/daily-review'), { date });
  }

  getEvents(): Observable<RecordedEvent[]> {
    return this.http.get<RecordedEvent[]>(apiUrl('/api/system/events'));
  }
}
