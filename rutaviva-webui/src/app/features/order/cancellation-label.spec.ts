import { cancellationLabel } from './cancellation-label';

describe('cancellationLabel', () => {
  it('explains a quote the client rejected', () => {
    expect(cancellationLabel('QuoteRejected')).toBe('Cotización rechazada');
  });

  it('explains an order the client cancelled', () => {
    expect(cancellationLabel('CancelledByClient')).toBe('Cancelada por el cliente');
  });
});
