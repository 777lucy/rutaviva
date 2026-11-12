using MediatR;
using RutaViva.Application.Commands.Shared;
using RutaViva.Domain.Aggregates;
using RutaViva.Domain.Interfaces;

namespace RutaViva.Application.Events;

internal sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IPublisher _publisher;
    private readonly IDomainEventFlusher _events;

    public DomainEventDispatcher(IPublisher publisher, IDomainEventFlusher events)
    {
        _publisher = publisher;
        _events = events;
    }

    public async Task DispatchAsync(AggregateRoot aggregate, CancellationToken cancellationToken)
    {
        foreach (IDomainEvent domainEvent in aggregate.DomainEvents.ToArray())
        {
            await _publisher.Publish((object)domainEvent, cancellationToken);
        }

        await _events.FlushAsync([aggregate], cancellationToken);
    }
}
