namespace RutaViva.Domain.ValueObjects;

public sealed record Quote(Money Amount, DateTimeOffset SubmittedAt);
