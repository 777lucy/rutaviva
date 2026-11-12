namespace RutaViva.Domain.ValueObjects.Identifiers;

public readonly record struct ServiceOrderId(Guid Value)
{
    public static ServiceOrderId New() => new(Guid.NewGuid());
}
