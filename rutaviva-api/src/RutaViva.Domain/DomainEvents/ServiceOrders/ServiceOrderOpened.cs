using RutaViva.Domain.Interfaces;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Domain.DomainEvents.ServiceOrders;

public sealed record ServiceOrderOpened(
    ServiceOrderId OrderId,
    VehicleId VehicleId,
    RoutineId RoutineId,
    WorkshopId WorkshopId) : IDomainEvent;
