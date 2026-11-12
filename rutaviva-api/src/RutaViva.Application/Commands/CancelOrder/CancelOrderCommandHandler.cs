using RutaViva.Application.Abstractions.Messaging;
using RutaViva.Application.Commands.Shared;
using RutaViva.Application.Ports;

namespace RutaViva.Application.Commands.CancelOrder;

internal sealed class CancelOrderCommandHandler : ICommandHandler<CancelOrderCommand>
{
    private readonly IOrderWriteAccess _access;
    private readonly IClock _clock;

    public CancelOrderCommandHandler(IOrderWriteAccess access, IClock clock)
    {
        _access = access;
        _clock = clock;
    }

    public Task Handle(CancelOrderCommand command, CancellationToken cancellationToken) =>
        _access.ChangeAsync(command.OrderId, order => order.Cancel(_clock.Now), cancellationToken);
}
