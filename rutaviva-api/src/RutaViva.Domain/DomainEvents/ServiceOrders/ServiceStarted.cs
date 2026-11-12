using RutaViva.Domain.Interfaces;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Domain.DomainEvents.ServiceOrders;

public sealed record ServiceStarted(ServiceOrderId OrderId) : IDomainEvent;
