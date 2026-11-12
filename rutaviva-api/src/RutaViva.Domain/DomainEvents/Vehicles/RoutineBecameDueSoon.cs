using RutaViva.Domain.Interfaces;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Domain.DomainEvents.Vehicles;

public sealed record RoutineBecameDueSoon(VehicleId VehicleId, FleetClientId ClientId, RoutineId RoutineId) : IDomainEvent;
