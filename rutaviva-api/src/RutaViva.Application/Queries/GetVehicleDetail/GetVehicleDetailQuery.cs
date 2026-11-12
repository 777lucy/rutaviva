using RutaViva.Application.Abstractions.Messaging;

namespace RutaViva.Application.Queries.GetVehicleDetail;

public sealed record GetVehicleDetailQuery(Guid VehicleId) : IQuery<VehicleDetailDto>;
