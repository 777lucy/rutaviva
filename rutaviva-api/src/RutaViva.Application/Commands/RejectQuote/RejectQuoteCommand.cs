using RutaViva.Application.Abstractions.Messaging;

namespace RutaViva.Application.Commands.RejectQuote;

public sealed record RejectQuoteCommand(Guid OrderId) : ICommand;
