using RutaViva.Domain.Aggregates;

namespace RutaViva.Application.Events;

internal interface IDomainEventDispatcher
{
    Task DispatchAsync(AggregateRoot aggregate, CancellationToken cancellationToken);
}
