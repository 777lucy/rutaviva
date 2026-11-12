using RutaViva.Application.Ports;
using RutaViva.Domain.ValueObjects.Identifiers;
using RutaViva.Infrastructure.Seed;

namespace RutaViva.Infrastructure.Clients;

public sealed class CurrentClient : ICurrentClient
{
    public FleetClientId Id => DemoFleet.Client;
}
