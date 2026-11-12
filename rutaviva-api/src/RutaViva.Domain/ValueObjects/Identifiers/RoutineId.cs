namespace RutaViva.Domain.ValueObjects.Identifiers;

public readonly record struct RoutineId(Guid Value)
{
    public static RoutineId New() => new(Guid.NewGuid());
}
