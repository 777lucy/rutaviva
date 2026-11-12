namespace RutaViva.Application.Ports;

public sealed record RecordedEvent(
    string Name,
    string Title,
    Guid? VehicleId,
    Guid? RoutineId,
    Guid? OrderId,
    DateTimeOffset OccurredAt);
