using RutaViva.Domain.Policies;
using RutaViva.Domain.Primitives;
using RutaViva.Domain.ValueObjects;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Domain.Aggregates.Vehicles;

public sealed class MaintenanceRoutine
{
    private MaintenanceRoutine(RoutineId id, string name, MaintenanceInterval interval, ServiceBaseline baseline)
    {
        Id = id;
        Name = name;
        Interval = interval;
        Baseline = baseline;
        DistanceStatus = interval.Distance is null ? DimensionStatus.NotApplicable : DimensionStatus.UpToDate;
        TimeStatus = interval.Months is null ? DimensionStatus.NotApplicable : DimensionStatus.UpToDate;
        Status = RoutineStatus.UpToDate;
    }

    public RoutineId Id { get; }

    public string Name { get; }

    public MaintenanceInterval Interval { get; }

    public ServiceBaseline Baseline { get; private set; }

    public DimensionStatus DistanceStatus { get; private set; }

    public DimensionStatus TimeStatus { get; private set; }

    public RoutineStatus Status { get; private set; }

    public static MaintenanceRoutine Create(RoutineId id, string name, MaintenanceInterval interval, ServiceBaseline baseline) =>
        new(id, name, interval, baseline);

    public bool ApplyDistance(Kilometers currentOdometer)
    {
        if (Interval.Distance is null)
        {
            return false;
        }

        DimensionStatus evaluatedDistanceStatus = RoutineProgressPolicy.EvaluateDistance(Baseline.Odometer, currentOdometer, Interval.Distance.Value);
        if (evaluatedDistanceStatus == DistanceStatus)
        {
            return false;
        }

        DistanceStatus = evaluatedDistanceStatus;
        return UpdateCombined();
    }

    public bool ApplyTime(DateOnly today)
    {
        if (Interval.Months is null)
        {
            return false;
        }

        DimensionStatus evaluatedTimeStatus = RoutineProgressPolicy.EvaluateTime(Baseline.PerformedOn, today, Interval.Months.Value);
        if (evaluatedTimeStatus == TimeStatus)
        {
            return false;
        }

        TimeStatus = evaluatedTimeStatus;
        return UpdateCombined();
    }

    public int? RemainingKilometers(Kilometers currentOdometer) =>
        Interval.Distance is null
            ? null
            : RoutineProgressPolicy.RemainingKilometers(Baseline.Odometer, currentOdometer, Interval.Distance.Value);

    public int? RemainingDays(DateOnly today) =>
        Interval.Months is null
            ? null
            : RoutineProgressPolicy.RemainingDays(Baseline.PerformedOn, today, Interval.Months.Value);

    public void Restart(ServiceCompletion completion)
    {
        Baseline = new ServiceBaseline(completion.Odometer, completion.PerformedOn);
        DistanceStatus = Interval.Distance is null ? DimensionStatus.NotApplicable : DimensionStatus.UpToDate;
        TimeStatus = Interval.Months is null ? DimensionStatus.NotApplicable : DimensionStatus.UpToDate;
        Status = RoutineStatus.UpToDate;
    }

    public void AlignTo(Kilometers currentOdometer, DateOnly today)
    {
        if (Interval.Distance is not null)
        {
            DistanceStatus = RoutineProgressPolicy.EvaluateDistance(Baseline.Odometer, currentOdometer, Interval.Distance.Value);
        }

        if (Interval.Months is not null)
        {
            TimeStatus = RoutineProgressPolicy.EvaluateTime(Baseline.PerformedOn, today, Interval.Months.Value);
        }

        Status = RoutineProgressPolicy.Combine(DistanceStatus, TimeStatus);
    }

    private bool UpdateCombined()
    {
        RoutineStatus combinedStatus = RoutineProgressPolicy.Combine(DistanceStatus, TimeStatus);
        if (combinedStatus == Status)
        {
            return false;
        }

        Status = combinedStatus;
        return true;
    }
}
