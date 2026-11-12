using RutaViva.Domain.Aggregates.ServiceOrders;
using RutaViva.Domain.Aggregates.Vehicles;

namespace RutaViva.Application.Commands.Shared;

internal interface IOrderWriteAccess
{
    Task ChangeAsync(Guid orderId, Action<ServiceOrder> change, CancellationToken cancellationToken);

    Task<IAsyncDisposable> LockAsync(Guid orderId, CancellationToken cancellationToken);

    Task<(ServiceOrder Order, Vehicle Vehicle)> LoadAsync(Guid orderId, CancellationToken cancellationToken);

    Task SaveAsync(ServiceOrder order, CancellationToken cancellationToken);

    Task SaveAsync(ServiceOrder order, Vehicle vehicle, CancellationToken cancellationToken);
}
