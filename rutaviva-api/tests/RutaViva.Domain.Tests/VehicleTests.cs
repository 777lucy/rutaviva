using RutaViva.Domain.Aggregates.Vehicles;
using RutaViva.Domain.DomainEvents.Vehicles;
using RutaViva.Domain.Primitives;
using RutaViva.Domain.ValueObjects;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Domain.Tests;

public sealed class VehicleTests
{
    private static readonly DateOnly BaselineDate = new(2026, 8, 1);

    [Fact]
    public void Higher_reading_is_accepted_and_lower_or_equal_readings_are_ignored()
    {
        Vehicle vehicle = Fleet();

        Assert.Equal(ReadingResult.Accepted, vehicle.RegisterOdometerReading(new Kilometers(59_000)));
        Assert.Equal(59_000, vehicle.Odometer.Value);
        Assert.Contains(vehicle.DomainEvents, domainEvent => domainEvent is OdometerAdvanced);

        vehicle.ClearDomainEvents();
        Assert.Equal(ReadingResult.Ignored, vehicle.RegisterOdometerReading(new Kilometers(59_000)));
        Assert.Equal(ReadingResult.Ignored, vehicle.RegisterOdometerReading(new Kilometers(58_000)));
        Assert.Equal(59_000, vehicle.Odometer.Value);
        Assert.Empty(vehicle.DomainEvents);
    }

    [Fact]
    public void Reading_does_not_change_time_status()
    {
        Vehicle vehicle = Fleet();
        MaintenanceRoutine routine = vehicle.Routines.Single();

        vehicle.RegisterOdometerReading(new Kilometers(70_000));

        Assert.Equal(DimensionStatus.UpToDate, routine.TimeStatus);
    }

    [Fact]
    public void Due_soon_and_overdue_are_signaled_once_per_cycle()
    {
        Vehicle vehicle = Fleet();

        vehicle.RegisterOdometerReading(new Kilometers(59_000));
        Assert.Single(vehicle.DomainEvents.OfType<RoutineBecameDueSoon>());

        vehicle.ClearDomainEvents();
        vehicle.RegisterOdometerReading(new Kilometers(59_500));
        Assert.Empty(vehicle.DomainEvents.OfType<RoutineBecameDueSoon>());

        vehicle.RegisterOdometerReading(new Kilometers(60_000));
        Assert.Single(vehicle.DomainEvents.OfType<RoutineBecameOverdue>());
        Assert.Empty(vehicle.DomainEvents.OfType<RoutineBecameDueSoon>());
    }

    [Fact]
    public void Jump_to_overdue_does_not_signal_due_soon()
    {
        Vehicle vehicle = Fleet();

        vehicle.RegisterOdometerReading(new Kilometers(61_000));

        Assert.Single(vehicle.DomainEvents.OfType<RoutineBecameOverdue>());
        Assert.Empty(vehicle.DomainEvents.OfType<RoutineBecameDueSoon>());
    }

    [Fact]
    public void Restart_clears_the_cycle_and_a_higher_reported_mileage_advances_the_odometer()
    {
        Vehicle vehicle = Fleet();
        vehicle.RegisterOdometerReading(new Kilometers(60_000));
        MaintenanceRoutine routine = vehicle.Routines.Single();
        vehicle.ClearDomainEvents();

        vehicle.RestartRoutine(routine.Id, new ServiceCompletion(new Kilometers(61_000), new DateOnly(2026, 11, 1)));

        Assert.Equal(RoutineStatus.UpToDate, routine.Status);
        Assert.Equal(61_000, vehicle.Odometer.Value);

        vehicle.RegisterOdometerReading(new Kilometers(70_000));
        Assert.Single(vehicle.DomainEvents.OfType<RoutineBecameDueSoon>());
    }

    [Fact]
    public void Daily_review_updates_time_and_not_the_odometer()
    {
        Vehicle vehicle = Fleet(MaintenanceInterval.Create(null, 6), new DateOnly(2026, 1, 1));
        Kilometers odometer = vehicle.Odometer;

        vehicle.RunDailyReview(new DateOnly(2026, 6, 13));

        Assert.Equal(odometer, vehicle.Odometer);
        Assert.Equal(RoutineStatus.DueSoon, vehicle.Routines.Single().Status);
        Assert.Single(vehicle.DomainEvents.OfType<RoutineBecameDueSoon>());
    }

    private static Vehicle Fleet() =>
        Fleet(MaintenanceInterval.Create(new Kilometers(10_000), 6), BaselineDate);

    private static Vehicle Fleet(MaintenanceInterval interval, DateOnly baseline)
    {
        Vehicle vehicle = Vehicle.Create(
            VehicleId.New(),
            FleetClientId.New(),
            WorkshopId.New(),
            "ABC-123",
            "Camión 3 t",
            new Kilometers(58_000));
        vehicle.AddRoutine(RoutineId.New(), "Aceite", interval, new ServiceBaseline(new Kilometers(50_000), baseline));
        return vehicle;
    }
}
