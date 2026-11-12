using RutaViva.Application.Abstractions.Messaging;

namespace RutaViva.Application.Commands.CompleteService;

public sealed record CompleteServiceCommand(Guid OrderId, int Kilometers, DateOnly? PerformedOn) : ICommand;
