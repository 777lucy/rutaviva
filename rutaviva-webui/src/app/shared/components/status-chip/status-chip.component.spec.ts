import { ComponentFixture, TestBed } from '@angular/core/testing';
import { StatusChipComponent } from './status-chip.component';

describe('StatusChipComponent', () => {
  let fixture: ComponentFixture<StatusChipComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [StatusChipComponent] }).compileComponents();
    fixture = TestBed.createComponent(StatusChipComponent);
  });

  it('renders the label it is given', () => {
    fixture.componentRef.setInput('label', 'Por vencer');
    fixture.componentRef.setInput('severity', 'warning');
    fixture.detectChanges();

    const chip: HTMLElement = fixture.nativeElement.querySelector('.status-chip');
    expect(chip.textContent?.trim()).toBe('Por vencer');
  });

  it('applies the class for its severity so color follows the status', () => {
    fixture.componentRef.setInput('label', 'Vencida');
    fixture.componentRef.setInput('severity', 'danger');
    fixture.detectChanges();

    const chip: HTMLElement = fixture.nativeElement.querySelector('.status-chip');
    expect(chip.classList).toContain('status-chip--danger');
  });
});
