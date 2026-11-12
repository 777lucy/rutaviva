using RutaViva.Application.Abstractions.Messaging;

namespace RutaViva.Application.Queries.GetFleetSummary;

public sealed record GetFleetSummaryQuery : IQuery<IReadOnlyList<FleetVehicleDto>>;
