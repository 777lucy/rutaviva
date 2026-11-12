using RutaViva.Application.Abstractions.Messaging;

namespace RutaViva.Application.Commands.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId) : ICommand;
