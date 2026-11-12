namespace RutaViva.Domain.ValueObjects.Identifiers;

public readonly record struct VehicleId(Guid Value)
{
    public static VehicleId New() => new(Guid.NewGuid());
}
