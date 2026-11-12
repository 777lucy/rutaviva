using RutaViva.Application.Ports;
using RutaViva.Domain.Aggregates.ServiceOrders;
using RutaViva.Domain.Aggregates.Vehicles;
using RutaViva.Domain.Policies;
using RutaViva.Domain.Primitives;
using RutaViva.Domain.ValueObjects;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Infrastructure.Seed;

public sealed class FleetSeeder
{
    private readonly IVehicleRepository _vehiclesRepository;
    private readonly IServiceOrderRepository _ordersRepository;
    private readonly IEventPublisher _publisher;
    private readonly IClock _clock;

    public FleetSeeder(
        IVehicleRepository vehiclesRepository,
        IServiceOrderRepository ordersRepository,
        IEventPublisher publisher,
        IClock clock)
    {
        _vehiclesRepository = vehiclesRepository;
        _ordersRepository = ordersRepository;
        _publisher = publisher;
        _clock = clock;
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await _vehiclesRepository.GetAsync(new VehicleId(DemoFleet.AbcVehicleId), cancellationToken) is not null)
        {
            return;
        }

        await _vehiclesRepository.SaveAsync(Aceite(), cancellationToken);
        Vehicle frenosVehicle = Frenos();
        await _vehiclesRepository.SaveAsync(frenosVehicle, cancellationToken);
        await _vehiclesRepository.SaveAsync(Llantas(), cancellationToken);
        await OpenFrenosOrderAsync(frenosVehicle, cancellationToken);
    }

    // La fecha base de aceite es agosto, no el 1 de enero: así el ejemplo de kilómetros
    // sigue vigente con el reloj real. El caso del 1 de enero vive en las pruebas de calendario.
    private Vehicle Aceite()
    {
        Vehicle vehicle = Vehicle.Create(
            new VehicleId(DemoFleet.AbcVehicleId),
            DemoFleet.Client,
            DemoFleet.Workshop,
            "ABC-123",
            "Camión 3 t",
            new Kilometers(58_000));
        vehicle.AddRoutine(
            new RoutineId(DemoFleet.AceiteRoutineId),
            "Aceite",
            MaintenanceInterval.Create(new Kilometers(10_000), 6),
            new ServiceBaseline(new Kilometers(50_000), new DateOnly(2026, 8, 1)));
        AddCurrentRoutine(vehicle, DemoFleet.AbcLlantasRoutineId, "Llantas", MaintenanceInterval.Create(new Kilometers(40_000), null));
        vehicle.AlignProgress(_clock.Today);
        return vehicle;
    }

    private Vehicle Frenos()
    {
        Vehicle vehicle = Vehicle.Create(
            new VehicleId(DemoFleet.XyzVehicleId),
            DemoFleet.Client,
            DemoFleet.Workshop,
            "XYZ-789",
            "Camión 5 t",
            new Kilometers(102_100));
        vehicle.AddRoutine(
            new RoutineId(DemoFleet.FrenosRoutineId),
            "Frenos",
            MaintenanceInterval.Create(null, 6),
            new ServiceBaseline(new Kilometers(102_100), new DateOnly(2026, 1, 1)));
        AddCurrentRoutine(vehicle, DemoFleet.XyzAceiteRoutineId, "Aceite", MaintenanceInterval.Create(new Kilometers(10_000), 6));
        AddCurrentRoutine(vehicle, DemoFleet.XyzLlantasRoutineId, "Llantas", MaintenanceInterval.Create(new Kilometers(40_000), null));
        vehicle.AlignProgress(_clock.Today);
        return vehicle;
    }

    private Vehicle Llantas()
    {
        Vehicle vehicle = Vehicle.Create(
            new VehicleId(DemoFleet.QweVehicleId),
            DemoFleet.Client,
            DemoFleet.Workshop,
            "QWE-456",
            "Van",
            new Kilometers(21_300));
        vehicle.AddRoutine(
            new RoutineId(DemoFleet.LlantasRoutineId),
            "Llantas",
            MaintenanceInterval.Create(new Kilometers(15_000), null),
            new ServiceBaseline(new Kilometers(10_000), new DateOnly(2026, 1, 1)));
        AddCurrentRoutine(vehicle, DemoFleet.QweAceiteRoutineId, "Aceite", MaintenanceInterval.Create(new Kilometers(10_000), 6));
        vehicle.AlignProgress(_clock.Today);
        return vehicle;
    }

    // La base es el odómetro actual y hoy, así la rutina nace al día.
    private void AddCurrentRoutine(Vehicle vehicle, Guid routineId, string name, MaintenanceInterval interval)
    {
        vehicle.AddRoutine(
            new RoutineId(routineId),
            name,
            interval,
            new ServiceBaseline(vehicle.Odometer, _clock.Today));
    }

    private async Task OpenFrenosOrderAsync(Vehicle frenos, CancellationToken cancellationToken)
    {
        MaintenanceRoutine frenosRoutine = frenos.Routines.Single(routine => routine.Id == new RoutineId(DemoFleet.FrenosRoutineId));
        if (frenosRoutine.Status != RoutineStatus.Overdue)
        {
            return;
        }

        ServiceOrder order = ServiceOrder.Open(
            new ServiceOrderId(DemoFleet.FrenosOrderId),
            frenos.Id,
            new RoutineId(DemoFleet.FrenosRoutineId),
            "Frenos",
            DemoFleet.Workshop,
            _clock.Now);
        await _ordersRepository.SaveAsync(order, cancellationToken);
        await _publisher.PublishAsync(order.DomainEvents, cancellationToken);
        order.ClearDomainEvents();
    }
}
