namespace RutaViva.Domain.ValueObjects.Identifiers;

public readonly record struct FleetClientId(Guid Value)
{
    public static FleetClientId New() => new(Guid.NewGuid());
}
