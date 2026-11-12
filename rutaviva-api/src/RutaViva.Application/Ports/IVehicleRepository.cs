using RutaViva.Domain.Aggregates.Vehicles;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Application.Ports;

public interface IVehicleRepository
{
    Task<Vehicle?> GetAsync(VehicleId id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Vehicle>> ListByClientAsync(FleetClientId clientId, CancellationToken cancellationToken);

    Task SaveAsync(Vehicle vehicle, CancellationToken cancellationToken);
}
