import { Injectable, effect, signal, untracked } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class RefreshService {
  readonly version = signal(0);

  notifyCommandSucceeded(): void {
    this.version.update((current) => current + 1);
  }
}

export function reloadOnRefresh(refresh: RefreshService, reload: () => void): void {
  let lastVersion = refresh.version();
  effect(() => {
    const version = refresh.version();
    if (version !== lastVersion) {
      lastVersion = version;
      untracked(reload);
    }
  });
}
