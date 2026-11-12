import { clientActions } from './order-actions';

describe('clientActions', () => {
  it('offers only cancel while the workshop has not quoted yet', () => {
    expect(clientActions('Created')).toEqual(['cancel']);
  });

  it('offers approve, reject, and cancel on a quoted order', () => {
    expect(clientActions('Quoted')).toEqual(['approve', 'reject', 'cancel']);
  });

  it('offers only cancel on an approved order', () => {
    expect(clientActions('Approved')).toEqual(['cancel']);
  });

  it('offers no client action once work has started or the order is closed', () => {
    expect(clientActions('InProgress')).toEqual([]);
    expect(clientActions('Completed')).toEqual([]);
    expect(clientActions('Cancelled')).toEqual([]);
  });
});
