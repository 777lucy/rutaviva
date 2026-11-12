using System.Collections.Concurrent;
using RutaViva.Application.Ports;
using RutaViva.Domain.Aggregates.Vehicles;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Infrastructure.Persistence;

public sealed class InMemoryVehicleRepository : IVehicleRepository
{
    private readonly ConcurrentDictionary<Guid, Vehicle> _vehicles = new();

    public Task<Vehicle?> GetAsync(VehicleId id, CancellationToken cancellationToken) =>
        Task.FromResult(_vehicles.GetValueOrDefault(id.Value));

    public Task<IReadOnlyList<Vehicle>> ListByClientAsync(FleetClientId clientId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Vehicle>>(_vehicles.Values.Where(vehicle => vehicle.ClientId == clientId).ToArray());

    public Task SaveAsync(Vehicle vehicle, CancellationToken cancellationToken)
    {
        _vehicles[vehicle.Id.Value] = vehicle;
        return Task.CompletedTask;
    }
}
