using RutaViva.Application.Errors;
using RutaViva.Application.Ports;
using RutaViva.Domain.Aggregates.ServiceOrders;
using RutaViva.Domain.Aggregates.Vehicles;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Application.Commands.Shared;

internal sealed class OrderWriteAccess : IOrderWriteAccess
{
    private const string OrderNotFound = "No se encontró la orden.";

    private readonly IServiceOrderRepository _ordersRepository;
    private readonly IVehicleRepository _vehiclesRepository;
    private readonly ICurrentClient _client;
    private readonly IVehicleLock _lock;
    private readonly IDomainEventFlusher _events;

    public OrderWriteAccess(
        IServiceOrderRepository ordersRepository,
        IVehicleRepository vehiclesRepository,
        ICurrentClient client,
        IVehicleLock vehicleLock,
        IDomainEventFlusher events)
    {
        _ordersRepository = ordersRepository;
        _vehiclesRepository = vehiclesRepository;
        _client = client;
        _lock = vehicleLock;
        _events = events;
    }

    public async Task ChangeAsync(Guid orderId, Action<ServiceOrder> change, CancellationToken cancellationToken)
    {
        await using (await LockAsync(orderId, cancellationToken))
        {
            (ServiceOrder order, _) = await LoadAsync(orderId, cancellationToken);
            change(order);
            await SaveAsync(order, cancellationToken);
        }
    }

    public async Task<IAsyncDisposable> LockAsync(Guid orderId, CancellationToken cancellationToken)
    {
        ServiceOrder order = await _ordersRepository.GetAsync(new ServiceOrderId(orderId), cancellationToken)
            ?? throw new NotFoundException(OrderNotFound);
        return await _lock.AcquireAsync(order.VehicleId, cancellationToken);
    }

    public async Task<(ServiceOrder Order, Vehicle Vehicle)> LoadAsync(Guid orderId, CancellationToken cancellationToken)
    {
        ServiceOrder order = await _ordersRepository.GetAsync(new ServiceOrderId(orderId), cancellationToken)
            ?? throw new NotFoundException(OrderNotFound);
        Vehicle? vehicle = await _vehiclesRepository.GetAsync(order.VehicleId, cancellationToken);
        if (vehicle is null || vehicle.ClientId != _client.Id)
        {
            throw new NotFoundException(OrderNotFound);
        }

        return (order, vehicle);
    }

    public async Task SaveAsync(ServiceOrder order, CancellationToken cancellationToken)
    {
        await _ordersRepository.SaveAsync(order, cancellationToken);
        await _events.FlushAsync([order], cancellationToken);
    }

    public async Task SaveAsync(ServiceOrder order, Vehicle vehicle, CancellationToken cancellationToken)
    {
        await _ordersRepository.SaveAsync(order, cancellationToken);
        await _vehiclesRepository.SaveAsync(vehicle, cancellationToken);
        await _events.FlushAsync([order, vehicle], cancellationToken);
    }
}
