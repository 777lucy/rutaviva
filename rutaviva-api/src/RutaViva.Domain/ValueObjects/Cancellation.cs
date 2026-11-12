using RutaViva.Domain.Primitives;

namespace RutaViva.Domain.ValueObjects;

public sealed record Cancellation(CancellationReason Reason, DateTimeOffset At);
