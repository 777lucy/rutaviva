using RutaViva.Domain.Interfaces;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Domain.DomainEvents.ServiceOrders;

public sealed record ServiceOrderCancelled(ServiceOrderId OrderId) : IDomainEvent;
