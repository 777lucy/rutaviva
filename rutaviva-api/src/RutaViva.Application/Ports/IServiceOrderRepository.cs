using RutaViva.Domain.Aggregates.ServiceOrders;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Application.Ports;

public interface IServiceOrderRepository
{
    Task<ServiceOrder?> GetAsync(ServiceOrderId id, CancellationToken cancellationToken);

    Task<IReadOnlyList<ServiceOrder>> ListByVehicleAsync(VehicleId vehicleId, CancellationToken cancellationToken);

    Task<bool> HasOpenOrderAsync(VehicleId vehicleId, RoutineId routineId, CancellationToken cancellationToken);

    Task SaveAsync(ServiceOrder order, CancellationToken cancellationToken);
}
