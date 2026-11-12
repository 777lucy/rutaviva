using RutaViva.Domain.Interfaces;
using RutaViva.Domain.Primitives;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Domain.DomainEvents.ServiceOrders;

public sealed record ServiceOrderApproved(ServiceOrderId OrderId, ApprovalSource Source) : IDomainEvent;
