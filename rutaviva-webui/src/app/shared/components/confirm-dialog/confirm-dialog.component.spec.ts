import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ConfirmDialogComponent } from './confirm-dialog.component';

describe('ConfirmDialogComponent', () => {
  let fixture: ComponentFixture<ConfirmDialogComponent>;
  let element: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [ConfirmDialogComponent] }).compileComponents();
    fixture = TestBed.createComponent(ConfirmDialogComponent);
    fixture.componentRef.setInput('title', '¿Rechazar la cotización?');
    fixture.componentRef.setInput('message', 'La orden de Frenos para XYZ-789 quedará cancelada.');
    fixture.componentRef.setInput('detail', 'La rutina sigue vencida. Esta versión no abre otra orden.');
    fixture.componentRef.setInput('confirmLabel', 'Sí, rechazar');
    fixture.detectChanges();
    element = fixture.nativeElement;
  });

  it('emits confirmed when the user confirms', () => {
    const confirmed = jasmine.createSpy('confirmed');
    fixture.componentInstance.confirmed.subscribe(confirmed);

    element.querySelector<HTMLButtonElement>('.confirm-dialog__confirm')?.click();

    expect(confirmed).toHaveBeenCalledTimes(1);
  });

  it('does not emit confirmed when the user cancels', () => {
    const confirmed = jasmine.createSpy('confirmed');
    fixture.componentInstance.confirmed.subscribe(confirmed);

    element.querySelector<HTMLButtonElement>('.confirm-dialog__cancel')?.click();

    expect(confirmed).not.toHaveBeenCalled();
  });

  it('labels the dismiss button so it is not confused with cancelling the order', () => {
    expect(element.querySelector('.confirm-dialog__cancel')?.textContent?.trim()).toBe('Volver');
  });

  it('shows the message and the secondary detail line', () => {
    expect(element.querySelector('.confirm-dialog__message')?.textContent?.trim()).toBe(
      'La orden de Frenos para XYZ-789 quedará cancelada.'
    );
    expect(element.querySelector('.confirm-dialog__detail')?.textContent?.trim()).toBe(
      'La rutina sigue vencida. Esta versión no abre otra orden.'
    );
  });

  it('outlines the confirm button in red by default and fills it for a primary tone', () => {
    const confirm = () => element.querySelector('.confirm-dialog__confirm');
    expect(confirm()?.classList).toContain('button--danger');

    fixture.componentRef.setInput('tone', 'primary');
    fixture.detectChanges();

    expect(confirm()?.classList).toContain('button--primary');
  });
});
