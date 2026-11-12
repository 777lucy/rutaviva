using RutaViva.Domain.Aggregates;
using RutaViva.Domain.DomainEvents.Vehicles;
using RutaViva.Domain.Exceptions;
using RutaViva.Domain.Primitives;
using RutaViva.Domain.ValueObjects;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Domain.Aggregates.Vehicles;

public sealed class Vehicle : AggregateRoot
{
    private readonly List<MaintenanceRoutine> _routines = [];

    private Vehicle(
        VehicleId id,
        FleetClientId clientId,
        WorkshopId assignedWorkshopId,
        string plate,
        string description,
        Kilometers odometer)
    {
        Id = id;
        ClientId = clientId;
        AssignedWorkshopId = assignedWorkshopId;
        Plate = plate;
        Description = description;
        Odometer = odometer;
    }

    public VehicleId Id { get; }

    public FleetClientId ClientId { get; }

    public WorkshopId AssignedWorkshopId { get; }

    public string Plate { get; }

    public string Description { get; }

    public Kilometers Odometer { get; private set; }

    public IReadOnlyList<MaintenanceRoutine> Routines => _routines;

    public static Vehicle Create(
        VehicleId id,
        FleetClientId clientId,
        WorkshopId assignedWorkshopId,
        string plate,
        string description,
        Kilometers odometer) =>
        new(id, clientId, assignedWorkshopId, plate, description, odometer);

    public MaintenanceRoutine AddRoutine(RoutineId id, string name, MaintenanceInterval interval, ServiceBaseline baseline)
    {
        MaintenanceRoutine routine = MaintenanceRoutine.Create(id, name, interval, baseline);
        _routines.Add(routine);
        return routine;
    }

    public ReadingResult RegisterOdometerReading(Kilometers reading)
    {
        if (!reading.IsGreaterThan(Odometer))
        {
            return ReadingResult.Ignored;
        }

        Odometer = reading;
        Raise(new OdometerAdvanced(Id, Odometer.Value));
        foreach (MaintenanceRoutine routine in _routines)
        {
            if (routine.ApplyDistance(Odometer))
            {
                RaiseStatus(routine);
            }
        }

        return ReadingResult.Accepted;
    }

    public void RunDailyReview(DateOnly today)
    {
        foreach (MaintenanceRoutine routine in _routines)
        {
            if (routine.ApplyTime(today))
            {
                RaiseStatus(routine);
            }
        }
    }

    public void RestartRoutine(RoutineId routineId, ServiceCompletion completion)
    {
        MaintenanceRoutine? routine = _routines.SingleOrDefault(candidate => candidate.Id == routineId)
            ?? throw new DomainException("routine.missing", "No se encontró la rutina.");

        if (completion.Odometer.IsGreaterThan(Odometer))
        {
            Odometer = completion.Odometer;
        }

        routine.Restart(completion);
    }

    public void AlignProgress(DateOnly today)
    {
        foreach (MaintenanceRoutine routine in _routines)
        {
            routine.AlignTo(Odometer, today);
        }
    }

    private void RaiseStatus(MaintenanceRoutine routine)
    {
        if (routine.Status == RoutineStatus.DueSoon)
        {
            Raise(new RoutineBecameDueSoon(Id, ClientId, routine.Id));
        }

        if (routine.Status == RoutineStatus.Overdue)
        {
            Raise(new RoutineBecameOverdue(Id, routine.Id, routine.Name, AssignedWorkshopId));
        }
    }
}
