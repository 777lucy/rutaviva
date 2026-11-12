using RutaViva.Domain.Interfaces;

namespace RutaViva.Application.Ports;

public interface IEventPublisher
{
    Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken);
}
