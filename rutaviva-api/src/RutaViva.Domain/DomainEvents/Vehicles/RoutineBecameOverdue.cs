using RutaViva.Domain.Interfaces;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Domain.DomainEvents.Vehicles;

public sealed record RoutineBecameOverdue(
    VehicleId VehicleId,
    RoutineId RoutineId,
    string RoutineName,
    WorkshopId WorkshopId) : IDomainEvent;
