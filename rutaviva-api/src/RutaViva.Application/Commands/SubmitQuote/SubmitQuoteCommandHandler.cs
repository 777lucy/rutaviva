using RutaViva.Application.Abstractions.Messaging;
using RutaViva.Application.Commands.Shared;
using RutaViva.Application.Ports;
using RutaViva.Domain.ValueObjects;

namespace RutaViva.Application.Commands.SubmitQuote;

internal sealed class SubmitQuoteCommandHandler : ICommandHandler<SubmitQuoteCommand>
{
    private readonly IOrderWriteAccess _access;
    private readonly IClock _clock;

    public SubmitQuoteCommandHandler(IOrderWriteAccess access, IClock clock)
    {
        _access = access;
        _clock = clock;
    }

    public Task Handle(SubmitQuoteCommand command, CancellationToken cancellationToken)
    {
        return _access.ChangeAsync(
            command.OrderId,
            order => order.SubmitQuote(new Money(command.Amount), _clock.Now),
            cancellationToken);
    }
}
