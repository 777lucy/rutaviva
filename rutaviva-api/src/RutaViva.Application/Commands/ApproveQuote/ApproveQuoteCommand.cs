using RutaViva.Application.Abstractions.Messaging;

namespace RutaViva.Application.Commands.ApproveQuote;

public sealed record ApproveQuoteCommand(Guid OrderId) : ICommand;
