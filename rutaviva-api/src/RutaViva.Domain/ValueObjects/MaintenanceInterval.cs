using RutaViva.Domain.Exceptions;

namespace RutaViva.Domain.ValueObjects;

public sealed record MaintenanceInterval
{
    private MaintenanceInterval(Kilometers? distance, int? months)
    {
        Distance = distance;
        Months = months;
    }

    public Kilometers? Distance { get; }

    public int? Months { get; }

    public static MaintenanceInterval Create(Kilometers? distance, int? months)
    {
        bool hasDistance = distance is not null;
        bool hasMonths = months is not null;
        if (!hasDistance && !hasMonths)
        {
            throw new DomainException("interval.missing", "La rutina necesita un intervalo en kilómetros, en meses, o en ambos.");
        }

        if (hasDistance && distance!.Value.Value == 0)
        {
            throw new DomainException("interval.distance", "El intervalo en kilómetros debe ser mayor que cero.");
        }

        if (hasMonths && months <= 0)
        {
            throw new DomainException("interval.months", "El intervalo en meses debe ser mayor que cero.");
        }

        return new MaintenanceInterval(distance, months);
    }
}
