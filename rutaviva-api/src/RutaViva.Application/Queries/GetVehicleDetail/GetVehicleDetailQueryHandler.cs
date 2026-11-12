using RutaViva.Application.Abstractions.Messaging;
using RutaViva.Application.Errors;
using RutaViva.Application.Ports;
using RutaViva.Application.Queries.Shared;
using RutaViva.Domain.Aggregates.ServiceOrders;
using RutaViva.Domain.Aggregates.Vehicles;
using RutaViva.Domain.ValueObjects;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Application.Queries.GetVehicleDetail;

internal sealed class GetVehicleDetailQueryHandler : IQueryHandler<GetVehicleDetailQuery, VehicleDetailDto>
{
    private readonly IVehicleRepository _vehiclesRepository;
    private readonly IServiceOrderRepository _ordersRepository;
    private readonly IWorkshopDirectory _workshops;
    private readonly ICurrentClient _client;
    private readonly IClock _clock;

    public GetVehicleDetailQueryHandler(
        IVehicleRepository vehiclesRepository,
        IServiceOrderRepository ordersRepository,
        IWorkshopDirectory workshops,
        ICurrentClient client,
        IClock clock)
    {
        _vehiclesRepository = vehiclesRepository;
        _ordersRepository = ordersRepository;
        _workshops = workshops;
        _client = client;
        _clock = clock;
    }

    public async Task<VehicleDetailDto> Handle(GetVehicleDetailQuery query, CancellationToken cancellationToken)
    {
        Vehicle? vehicle = await _vehiclesRepository.GetAsync(new VehicleId(query.VehicleId), cancellationToken);
        if (vehicle is null || vehicle.ClientId != _client.Id)
        {
            throw new NotFoundException("No se encontró el vehículo.");
        }

        IReadOnlyList<ServiceOrder> orders = await _ordersRepository.ListByVehicleAsync(vehicle.Id, cancellationToken);
        DateOnly today = _clock.Today;
        return new VehicleDetailDto(
            vehicle.Id.Value,
            vehicle.Plate,
            vehicle.Description,
            vehicle.Odometer.Value,
            _workshops.GetName(vehicle.AssignedWorkshopId),
            vehicle.Routines.Select(routine => ToDto(routine, orders, vehicle.Odometer, today)).ToArray());
    }

    private static VehicleRoutineDto ToDto(
        MaintenanceRoutine routine,
        IReadOnlyList<ServiceOrder> orders,
        Kilometers odometer,
        DateOnly today)
    {
        ServiceOrder? openOrder = orders.FirstOrDefault(order => order.RoutineId == routine.Id && order.IsOpen);
        return new VehicleRoutineDto(
            routine.Id.Value,
            routine.Name,
            IntervalText.Format(routine.Interval),
            routine.Status.ToString(),
            openOrder is null ? null : new OpenOrderDto(openOrder.Id.Value, openOrder.Status.ToString()),
            new RoutineBaselineDto(routine.Baseline.Odometer.Value, routine.Baseline.PerformedOn),
            new RoutineRemainingDto(routine.RemainingKilometers(odometer), routine.RemainingDays(today)));
    }
}
