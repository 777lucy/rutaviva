namespace RutaViva.Application.Commands.RunDailyReview;

public sealed record RoutineChange(Guid VehicleId, Guid RoutineId, string Status);
