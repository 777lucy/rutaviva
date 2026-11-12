using RutaViva.Application.Abstractions.Messaging;
using RutaViva.Application.Commands.Shared;
using RutaViva.Application.Ports;

namespace RutaViva.Application.Commands.RejectQuote;

internal sealed class RejectQuoteCommandHandler : ICommandHandler<RejectQuoteCommand>
{
    private readonly IOrderWriteAccess _access;
    private readonly IClock _clock;

    public RejectQuoteCommandHandler(IOrderWriteAccess access, IClock clock)
    {
        _access = access;
        _clock = clock;
    }

    public Task Handle(RejectQuoteCommand command, CancellationToken cancellationToken) =>
        _access.ChangeAsync(command.OrderId, order => order.RejectQuote(_clock.Now), cancellationToken);
}
