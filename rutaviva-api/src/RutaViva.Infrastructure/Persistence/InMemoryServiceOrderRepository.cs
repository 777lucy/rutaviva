using System.Collections.Concurrent;
using RutaViva.Application.Ports;
using RutaViva.Domain.Aggregates.ServiceOrders;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Infrastructure.Persistence;

public sealed class InMemoryServiceOrderRepository : IServiceOrderRepository
{
    private readonly ConcurrentDictionary<Guid, ServiceOrder> _orders = new();

    public Task<ServiceOrder?> GetAsync(ServiceOrderId id, CancellationToken cancellationToken) =>
        Task.FromResult(_orders.GetValueOrDefault(id.Value));

    public Task<IReadOnlyList<ServiceOrder>> ListByVehicleAsync(VehicleId vehicleId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ServiceOrder>>(_orders.Values.Where(order => order.VehicleId == vehicleId).ToArray());

    public Task<bool> HasOpenOrderAsync(VehicleId vehicleId, RoutineId routineId, CancellationToken cancellationToken) =>
        Task.FromResult(_orders.Values.Any(order => order.VehicleId == vehicleId && order.RoutineId == routineId && order.IsOpen));

    public Task SaveAsync(ServiceOrder order, CancellationToken cancellationToken)
    {
        _orders[order.Id.Value] = order;
        return Task.CompletedTask;
    }
}
