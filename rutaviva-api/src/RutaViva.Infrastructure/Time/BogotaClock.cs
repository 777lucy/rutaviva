using RutaViva.Application.Ports;

namespace RutaViva.Infrastructure.Time;

public sealed class BogotaClock : IClock
{
    private static readonly TimeZoneInfo Zone = ResolveZone();

    public DateTimeOffset Now => TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, Zone);

    public DateOnly Today => DateOnly.FromDateTime(Now.DateTime);

    private static TimeZoneInfo ResolveZone()
    {
        foreach (string timeZoneId in new[] { "America/Bogota", "SA Pacific Standard Time" })
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out TimeZoneInfo? timeZone))
            {
                return timeZone;
            }
        }

        return TimeZoneInfo.CreateCustomTimeZone("America/Bogota", TimeSpan.FromHours(-5), "Bogota", "Bogota");
    }
}
