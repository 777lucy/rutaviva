using RutaViva.Application.Abstractions.Messaging;
using RutaViva.Application.Ports;
using RutaViva.Domain.Aggregates.ServiceOrders;
using RutaViva.Domain.Aggregates.Vehicles;
using RutaViva.Domain.Primitives;

namespace RutaViva.Application.Queries.GetFleetSummary;

internal sealed class GetFleetSummaryQueryHandler : IQueryHandler<GetFleetSummaryQuery, IReadOnlyList<FleetVehicleDto>>
{
    private readonly IVehicleRepository _vehiclesRepository;
    private readonly IServiceOrderRepository _ordersRepository;
    private readonly ICurrentClient _client;

    public GetFleetSummaryQueryHandler(IVehicleRepository vehiclesRepository, IServiceOrderRepository ordersRepository, ICurrentClient client)
    {
        _vehiclesRepository = vehiclesRepository;
        _ordersRepository = ordersRepository;
        _client = client;
    }

    public async Task<IReadOnlyList<FleetVehicleDto>> Handle(GetFleetSummaryQuery query, CancellationToken cancellationToken)
    {
        List<FleetVehicleDto> fleet = new List<FleetVehicleDto>();
        foreach (Vehicle vehicle in await _vehiclesRepository.ListByClientAsync(_client.Id, cancellationToken))
        {
            IReadOnlyList<ServiceOrder> orders = await _ordersRepository.ListByVehicleAsync(vehicle.Id, cancellationToken);
            fleet.Add(ToDto(vehicle, orders));
        }

        return fleet;
    }

    private static FleetVehicleDto ToDto(Vehicle vehicle, IReadOnlyList<ServiceOrder> orders) =>
        new(
            vehicle.Id.Value,
            vehicle.Plate,
            vehicle.Description,
            vehicle.Odometer.Value,
            orders.Any(order => order.Status == ServiceOrderStatus.Quoted),
            vehicle.Routines
                .Select(routine => new FleetRoutineDto(routine.Id.Value, routine.Name, routine.Status.ToString()))
                .ToArray());
}
