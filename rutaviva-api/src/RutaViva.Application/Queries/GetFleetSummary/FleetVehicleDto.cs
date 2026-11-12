namespace RutaViva.Application.Queries.GetFleetSummary;

public sealed record FleetVehicleDto(
    Guid Id,
    string Plate,
    string Description,
    int Odometer,
    bool AwaitingQuote,
    IReadOnlyList<FleetRoutineDto> Routines);
