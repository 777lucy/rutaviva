namespace RutaViva.Application.Queries.GetVehicleDetail;

public sealed record VehicleRoutineDto(
    Guid Id,
    string Name,
    string Interval,
    string Status,
    OpenOrderDto? OpenOrder,
    RoutineBaselineDto Baseline,
    RoutineRemainingDto Remaining);
