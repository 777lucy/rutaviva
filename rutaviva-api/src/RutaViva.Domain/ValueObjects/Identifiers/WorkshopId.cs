namespace RutaViva.Domain.ValueObjects.Identifiers;

public readonly record struct WorkshopId(Guid Value)
{
    public static WorkshopId New() => new(Guid.NewGuid());
}
