using RutaViva.Application.Ports;
using RutaViva.Domain.Aggregates;

namespace RutaViva.Application.Commands.Shared;

internal sealed class DomainEventFlusher : IDomainEventFlusher
{
    private readonly IEventPublisher _publisher;

    public DomainEventFlusher(IEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task FlushAsync(IEnumerable<AggregateRoot> aggregates, CancellationToken cancellationToken)
    {
        AggregateRoot[] aggregatesToFlush = aggregates.ToArray();
        await _publisher.PublishAsync(aggregatesToFlush.SelectMany(aggregate => aggregate.DomainEvents).ToArray(), cancellationToken);
        foreach (AggregateRoot aggregate in aggregatesToFlush)
        {
            aggregate.ClearDomainEvents();
        }
    }
}
