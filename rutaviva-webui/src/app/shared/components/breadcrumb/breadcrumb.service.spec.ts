import { TestBed } from '@angular/core/testing';
import { BreadcrumbService } from './breadcrumb.service';

describe('BreadcrumbService', () => {
  it('starts at the fleet root', () => {
    const service = TestBed.inject(BreadcrumbService);

    expect(service.crumbs()).toEqual([{ label: 'Flota', link: null }]);
  });

  it('links every earlier crumb and leaves the current one as plain text', () => {
    const service = TestBed.inject(BreadcrumbService);

    service.show([
      { label: 'XYZ-789', link: ['/vehicles', 'vehicle-2'] },
      { label: 'Orden', link: null }
    ]);

    expect(service.crumbs()).toEqual([
      { label: 'Flota', link: ['/'] },
      { label: 'XYZ-789', link: ['/vehicles', 'vehicle-2'] },
      { label: 'Orden', link: null }
    ]);
  });
});
