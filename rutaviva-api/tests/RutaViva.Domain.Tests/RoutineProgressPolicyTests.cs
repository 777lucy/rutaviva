using RutaViva.Domain.Policies;
using RutaViva.Domain.Primitives;
using RutaViva.Domain.ValueObjects;

namespace RutaViva.Domain.Tests;

public sealed class RoutineProgressPolicyTests
{
    [Theory]
    [InlineData(8_000, DimensionStatus.UpToDate)]
    [InlineData(8_999, DimensionStatus.UpToDate)]
    [InlineData(9_000, DimensionStatus.DueSoon)]
    [InlineData(9_999, DimensionStatus.DueSoon)]
    [InlineData(10_000, DimensionStatus.Overdue)]
    [InlineData(11_000, DimensionStatus.Overdue)]
    public void Distance_uses_the_ten_percent_window(int elapsed, DimensionStatus expected)
    {
        DimensionStatus status = RoutineProgressPolicy.EvaluateDistance(
            new Kilometers(50_000),
            new Kilometers(50_000 + elapsed),
            new Kilometers(10_000));

        Assert.Equal(expected, status);
    }

    [Fact]
    public void Six_months_from_1_January_2026_uses_the_real_calendar()
    {
        DateOnly baseline = new DateOnly(2026, 1, 1);

        Assert.Equal(DimensionStatus.UpToDate, RoutineProgressPolicy.EvaluateTime(baseline, new DateOnly(2026, 6, 12), 6));
        Assert.Equal(DimensionStatus.DueSoon, RoutineProgressPolicy.EvaluateTime(baseline, new DateOnly(2026, 6, 13), 6));
        Assert.Equal(DimensionStatus.Overdue, RoutineProgressPolicy.EvaluateTime(baseline, new DateOnly(2026, 7, 1), 6));
    }

    [Fact]
    public void One_month_from_31_January_lands_on_28_February()
    {
        DateOnly baseline = new DateOnly(2026, 1, 31);

        Assert.Equal(DimensionStatus.UpToDate, RoutineProgressPolicy.EvaluateTime(baseline, new DateOnly(2026, 2, 25), 1));
        Assert.Equal(DimensionStatus.DueSoon, RoutineProgressPolicy.EvaluateTime(baseline, new DateOnly(2026, 2, 26), 1));
        Assert.Equal(DimensionStatus.Overdue, RoutineProgressPolicy.EvaluateTime(baseline, new DateOnly(2026, 2, 28), 1));
    }

    [Fact]
    public void Remaining_kilometers_is_the_interval_minus_the_elapsed_distance()
    {
        int remaining = RoutineProgressPolicy.RemainingKilometers(
            new Kilometers(10_000),
            new Kilometers(21_300),
            new Kilometers(15_000));

        Assert.Equal(3_700, remaining);
    }

    [Fact]
    public void Remaining_kilometers_becomes_negative_once_the_interval_is_exceeded()
    {
        int remaining = RoutineProgressPolicy.RemainingKilometers(
            new Kilometers(10_000),
            new Kilometers(26_000),
            new Kilometers(15_000));

        Assert.Equal(-1_000, remaining);
    }

    [Fact]
    public void Remaining_kilometers_counts_a_full_interval_when_the_odometer_is_below_the_baseline()
    {
        int remaining = RoutineProgressPolicy.RemainingKilometers(
            new Kilometers(10_000),
            new Kilometers(9_000),
            new Kilometers(15_000));

        Assert.Equal(15_000, remaining);
    }

    [Fact]
    public void Remaining_days_counts_calendar_days_to_the_due_date()
    {
        Assert.Equal(18, RoutineProgressPolicy.RemainingDays(new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 13), 6));
        Assert.Equal(-10, RoutineProgressPolicy.RemainingDays(new DateOnly(2026, 1, 1), new DateOnly(2026, 7, 11), 6));
    }

    [Fact]
    public void Remaining_days_counts_a_full_cycle_when_today_is_before_the_baseline()
    {
        Assert.Equal(181, RoutineProgressPolicy.RemainingDays(new DateOnly(2026, 1, 1), new DateOnly(2025, 12, 1), 6));
    }

    [Fact]
    public void Combined_status_is_the_worse_applicable_dimension()
    {
        Assert.Equal(RoutineStatus.DueSoon, RoutineProgressPolicy.Combine(DimensionStatus.DueSoon, DimensionStatus.UpToDate));
        Assert.Equal(RoutineStatus.Overdue, RoutineProgressPolicy.Combine(DimensionStatus.DueSoon, DimensionStatus.Overdue));
        Assert.Equal(RoutineStatus.DueSoon, RoutineProgressPolicy.Combine(DimensionStatus.NotApplicable, DimensionStatus.DueSoon));
        Assert.Equal(RoutineStatus.UpToDate, RoutineProgressPolicy.Combine(DimensionStatus.UpToDate, DimensionStatus.NotApplicable));
    }
}
