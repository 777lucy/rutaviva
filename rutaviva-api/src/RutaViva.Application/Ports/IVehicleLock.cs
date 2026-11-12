using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Application.Ports;

public interface IVehicleLock
{
    Task<IAsyncDisposable> AcquireAsync(VehicleId vehicleId, CancellationToken cancellationToken);
}
