using RutaViva.Domain.Interfaces;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Domain.DomainEvents.Vehicles;

public sealed record OdometerAdvanced(VehicleId VehicleId, int Kilometers) : IDomainEvent;
