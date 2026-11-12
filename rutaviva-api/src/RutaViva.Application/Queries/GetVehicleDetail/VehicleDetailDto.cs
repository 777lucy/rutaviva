namespace RutaViva.Application.Queries.GetVehicleDetail;

public sealed record VehicleDetailDto(
    Guid Id,
    string Plate,
    string Description,
    int Odometer,
    string WorkshopName,
    IReadOnlyList<VehicleRoutineDto> Routines);
