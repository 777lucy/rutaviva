using RutaViva.Application.Abstractions.Messaging;

namespace RutaViva.Application.Queries.GetOrderDetail;

public sealed record GetOrderDetailQuery(Guid OrderId) : IQuery<OrderDetailDto>;
