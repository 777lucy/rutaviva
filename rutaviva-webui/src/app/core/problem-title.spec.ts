import { HttpErrorResponse } from '@angular/common/http';
import { problemTitle } from './problem-title';

describe('problemTitle', () => {
  it('returns the title of a problem-details body', () => {
    const error = new HttpErrorResponse({
      status: 409,
      error: { title: 'La orden no está en estado Quoted.', status: 409 }
    });

    expect(problemTitle(error)).toBe('La orden no está en estado Quoted.');
  });

  it('returns a generic Spanish message when the body has no title', () => {
    const error = new HttpErrorResponse({ status: 500, error: 'Internal error' });

    expect(problemTitle(error)).toBe('No se pudo completar la operación.');
  });

  it('returns the same generic message for errors that are not HTTP responses', () => {
    expect(problemTitle(new Error('network down'))).toBe('No se pudo completar la operación.');
  });
});
