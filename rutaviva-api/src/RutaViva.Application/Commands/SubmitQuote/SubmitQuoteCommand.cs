using RutaViva.Application.Abstractions.Messaging;

namespace RutaViva.Application.Commands.SubmitQuote;

public sealed record SubmitQuoteCommand(Guid OrderId, decimal Amount) : ICommand;
