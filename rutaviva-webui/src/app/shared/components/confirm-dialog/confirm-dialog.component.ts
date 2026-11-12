import { ChangeDetectionStrategy, Component, ElementRef, computed, input, output, viewChild } from '@angular/core';

export type ConfirmTone = 'danger' | 'primary';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.scss'
})
export class ConfirmDialogComponent {
  readonly title = input.required<string>();
  readonly message = input.required<string>();
  readonly detail = input('');
  readonly confirmLabel = input.required<string>();
  readonly cancelLabel = input('Volver');
  readonly tone = input<ConfirmTone>('danger');

  readonly confirmed = output<void>();

  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');

  protected readonly confirmClass = computed(() => `button button--${this.tone()} confirm-dialog__confirm`);

  open(): void {
    this.dialog().nativeElement.showModal();
  }

  protected onConfirm(): void {
    this.dialog().nativeElement.close();
    this.confirmed.emit();
  }

  protected onCancel(): void {
    this.dialog().nativeElement.close();
  }
}
