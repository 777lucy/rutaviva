using RutaViva.Application.Abstractions.Messaging;

namespace RutaViva.Application.Commands.StartService;

public sealed record StartServiceCommand(Guid OrderId) : ICommand;
