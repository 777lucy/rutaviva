using System.Globalization;
using RutaViva.Domain.ValueObjects;

namespace RutaViva.Application.Queries.Shared;

internal static class IntervalText
{
    private static readonly CultureInfo Spanish = CultureInfo.GetCultureInfo("es-CO");

    public static string Format(MaintenanceInterval interval)
    {
        string? distance = interval.Distance is null ? null : $"Cada {interval.Distance.Value.Value.ToString("N0", Spanish)} km";
        string? months = interval.Months is null ? null : $"Cada {interval.Months} meses";
        if (distance is not null && months is not null)
        {
            return $"{distance} o {interval.Months} meses";
        }

        return distance ?? months ?? string.Empty;
    }
}
