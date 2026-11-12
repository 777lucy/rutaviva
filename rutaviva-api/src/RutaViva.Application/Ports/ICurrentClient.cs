using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Application.Ports;

public interface ICurrentClient
{
    FleetClientId Id { get; }
}
