using RutaViva.Application.Abstractions.Messaging;
using RutaViva.Application.Errors;
using RutaViva.Application.Ports;
using RutaViva.Domain.Aggregates.ServiceOrders;
using RutaViva.Domain.Aggregates.Vehicles;
using RutaViva.Domain.Primitives;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Application.Queries.GetOrderDetail;

internal sealed class GetOrderDetailQueryHandler : IQueryHandler<GetOrderDetailQuery, OrderDetailDto>
{
    private const string OrderNotFound = "No se encontró la orden.";
    private const string Pending = "Pendiente";

    private readonly IServiceOrderRepository _ordersRepository;
    private readonly IVehicleRepository _vehiclesRepository;
    private readonly IWorkshopDirectory _workshops;
    private readonly ICurrentClient _client;

    public GetOrderDetailQueryHandler(
        IServiceOrderRepository ordersRepository,
        IVehicleRepository vehiclesRepository,
        IWorkshopDirectory workshops,
        ICurrentClient client)
    {
        _ordersRepository = ordersRepository;
        _vehiclesRepository = vehiclesRepository;
        _workshops = workshops;
        _client = client;
    }

    public async Task<OrderDetailDto> Handle(GetOrderDetailQuery query, CancellationToken cancellationToken)
    {
        ServiceOrder order = await _ordersRepository.GetAsync(new ServiceOrderId(query.OrderId), cancellationToken)
            ?? throw new NotFoundException(OrderNotFound);
        Vehicle? vehicle = await _vehiclesRepository.GetAsync(order.VehicleId, cancellationToken);
        if (vehicle is null || vehicle.ClientId != _client.Id)
        {
            throw new NotFoundException(OrderNotFound);
        }

        return new OrderDetailDto(
            order.Id.Value,
            vehicle.Id.Value,
            vehicle.Plate,
            order.RoutineName,
            _workshops.GetName(order.WorkshopId),
            order.Status.ToString(),
            order.Quote?.Amount.Amount,
            order.Cancellation?.Reason.ToString(),
            order.Completion?.Odometer.Value,
            order.Completion?.PerformedOn,
            History(order));
    }

    private static IReadOnlyList<OrderHistoryItemDto> History(ServiceOrder order)
    {
        int reached = ReachedStep(order);
        return
        [
            new("Creada", "La rutina venció", true),
            new("Cotizada", order.Quote is null ? Pending : $"{order.Quote.Amount.Amount:0} COP", reached >= 1),
            new("Aprobada", reached >= 2 ? "Aprobada" : Pending, reached >= 2),
            new("En ejecución", reached >= 3 ? "El taller inició" : Pending, reached >= 3),
            new("Completada", reached >= 4 ? "Servicio reportado" : Pending, reached >= 4)
        ];
    }

    // Una orden cancelada solo muestra los pasos que alcanzó antes de cancelarse.
    private static int ReachedStep(ServiceOrder order) => order.Status switch
    {
        ServiceOrderStatus.Quoted => 1,
        ServiceOrderStatus.Approved => 2,
        ServiceOrderStatus.InProgress => 3,
        ServiceOrderStatus.Completed => 4,
        ServiceOrderStatus.Cancelled when order.Quote is not null => 1,
        _ => 0
    };
}
