using RutaViva.Domain.Aggregates;

namespace RutaViva.Application.Commands.Shared;

internal interface IDomainEventFlusher
{
    Task FlushAsync(IEnumerable<AggregateRoot> aggregates, CancellationToken cancellationToken);
}
