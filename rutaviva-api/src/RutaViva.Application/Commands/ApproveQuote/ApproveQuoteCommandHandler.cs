using RutaViva.Application.Abstractions.Messaging;
using RutaViva.Application.Commands.Shared;
using RutaViva.Application.Ports;

namespace RutaViva.Application.Commands.ApproveQuote;

internal sealed class ApproveQuoteCommandHandler : ICommandHandler<ApproveQuoteCommand>
{
    private readonly IOrderWriteAccess _access;
    private readonly IClock _clock;

    public ApproveQuoteCommandHandler(IOrderWriteAccess access, IClock clock)
    {
        _access = access;
        _clock = clock;
    }

    public Task Handle(ApproveQuoteCommand command, CancellationToken cancellationToken) =>
        _access.ChangeAsync(command.OrderId, order => order.Approve(_clock.Now), cancellationToken);
}
