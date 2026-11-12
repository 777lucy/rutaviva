import { TestBed } from '@angular/core/testing';
import { RefreshService } from './refresh.service';

describe('RefreshService', () => {
  it('starts at version 0 and increments on each successful command', () => {
    const refresh = TestBed.inject(RefreshService);

    expect(refresh.version()).toBe(0);

    refresh.notifyCommandSucceeded();
    refresh.notifyCommandSucceeded();

    expect(refresh.version()).toBe(2);
  });
});
