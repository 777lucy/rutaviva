import { workshopActions } from './workshop-actions';

describe('workshopActions', () => {
  it('lets the workshop quote a created order', () => {
    expect(workshopActions('Created')).toEqual(['quote']);
  });

  it('lets the workshop start an approved order', () => {
    expect(workshopActions('Approved')).toEqual(['start']);
  });

  it('lets the workshop complete an order in progress', () => {
    expect(workshopActions('InProgress')).toEqual(['complete']);
  });

  it('offers no workshop action for quoted, completed, or cancelled orders', () => {
    expect(workshopActions('Quoted')).toEqual([]);
    expect(workshopActions('Completed')).toEqual([]);
    expect(workshopActions('Cancelled')).toEqual([]);
  });
});
