using RutaViva.Domain.Primitives;
using RutaViva.Domain.ValueObjects;

namespace RutaViva.Domain.Policies;

public static class RoutineProgressPolicy
{
    public static DimensionStatus EvaluateDistance(Kilometers baseline, Kilometers currentOdometer, Kilometers interval)
    {
        int elapsedKilometers = currentOdometer.Value - baseline.Value;
        if (elapsedKilometers < 0)
        {
            return DimensionStatus.UpToDate;
        }

        return Evaluate(elapsedKilometers, interval.Value);
    }

    public static DimensionStatus EvaluateTime(DateOnly baseline, DateOnly today, int months)
    {
        DateOnly dueDate = baseline.AddMonths(months);
        int cycleLengthInDays = dueDate.DayNumber - baseline.DayNumber;
        int elapsedDays = today.DayNumber - baseline.DayNumber;
        if (elapsedDays < 0)
        {
            return DimensionStatus.UpToDate;
        }

        return Evaluate(elapsedDays, cycleLengthInDays);
    }

    public static int RemainingKilometers(Kilometers baseline, Kilometers currentOdometer, Kilometers interval)
    {
        int elapsedKilometers = Math.Max(0, currentOdometer.Value - baseline.Value);
        return interval.Value - elapsedKilometers;
    }

    public static int RemainingDays(DateOnly baseline, DateOnly today, int months)
    {
        DateOnly dueDate = baseline.AddMonths(months);
        int elapsedDays = Math.Max(0, today.DayNumber - baseline.DayNumber);
        return dueDate.DayNumber - baseline.DayNumber - elapsedDays;
    }

    public static RoutineStatus Combine(DimensionStatus distance, DimensionStatus time)
    {
        DimensionStatus mostSevere = DimensionStatus.NotApplicable;
        if (Rank(distance) > Rank(mostSevere))
        {
            mostSevere = distance;
        }

        if (Rank(time) > Rank(mostSevere))
        {
            mostSevere = time;
        }

        return mostSevere switch
        {
            DimensionStatus.DueSoon => RoutineStatus.DueSoon,
            DimensionStatus.Overdue => RoutineStatus.Overdue,
            _ => RoutineStatus.UpToDate
        };
    }

    private static DimensionStatus Evaluate(int elapsedAmount, int cycleLength)
    {
        if (elapsedAmount >= cycleLength)
        {
            return DimensionStatus.Overdue;
        }

        int remainingAmount = cycleLength - elapsedAmount;
        if (remainingAmount * 10L <= cycleLength)
        {
            return DimensionStatus.DueSoon;
        }

        return DimensionStatus.UpToDate;
    }

    private static int Rank(DimensionStatus status) =>
        status == DimensionStatus.NotApplicable ? 0 : (int)status;
}
