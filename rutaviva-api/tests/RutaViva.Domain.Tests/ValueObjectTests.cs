using RutaViva.Domain.Exceptions;
using RutaViva.Domain.ValueObjects;

namespace RutaViva.Domain.Tests;

public sealed class ValueObjectTests
{
    [Fact]
    public void Kilometers_reject_a_negative_value()
    {
        DomainException domainException = Assert.Throws<DomainException>(() => new Kilometers(-1));

        Assert.Equal("Los kilómetros no pueden ser negativos.", domainException.Message);
    }

    [Fact]
    public void Kilometers_compare_by_magnitude()
    {
        Kilometers higher = new Kilometers(59_000);
        Kilometers lower = new Kilometers(58_000);

        Assert.True(higher.IsGreaterThan(lower));
        Assert.False(lower.IsGreaterThan(higher));
        Assert.False(higher.IsGreaterThan(higher));
    }

    [Fact]
    public void Money_rejects_a_negative_amount()
    {
        DomainException domainException = Assert.Throws<DomainException>(() => new Money(-1m));

        Assert.Equal("El valor no puede ser negativo.", domainException.Message);
    }

    [Fact]
    public void Money_is_compared_by_amount()
    {
        Assert.True(new Money(1_800_000m).IsGreaterThan(new Money(1_500_000m)));
        Assert.False(new Money(1_500_000m).IsGreaterThan(new Money(1_500_000m)));
    }

    [Fact]
    public void Interval_requires_at_least_one_positive_side()
    {
        Assert.Throws<DomainException>(() => MaintenanceInterval.Create(null, null));
        Assert.Throws<DomainException>(() => MaintenanceInterval.Create(new Kilometers(0), null));
        Assert.Throws<DomainException>(() => MaintenanceInterval.Create(null, 0));
    }

    [Fact]
    public void Interval_accepts_kilometers_months_or_both()
    {
        MaintenanceInterval distanceOnly = MaintenanceInterval.Create(new Kilometers(10_000), null);
        MaintenanceInterval monthsOnly = MaintenanceInterval.Create(null, 6);
        MaintenanceInterval distanceAndMonths = MaintenanceInterval.Create(new Kilometers(10_000), 6);

        Assert.Equal(10_000, distanceOnly.Distance!.Value.Value);
        Assert.Null(distanceOnly.Months);
        Assert.Null(monthsOnly.Distance);
        Assert.Equal(6, monthsOnly.Months);
        Assert.Equal(6, distanceAndMonths.Months);
    }
}
