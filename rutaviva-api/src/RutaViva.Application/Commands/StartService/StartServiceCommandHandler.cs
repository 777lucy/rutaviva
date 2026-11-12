using RutaViva.Application.Abstractions.Messaging;
using RutaViva.Application.Commands.Shared;
using RutaViva.Application.Ports;

namespace RutaViva.Application.Commands.StartService;

internal sealed class StartServiceCommandHandler : ICommandHandler<StartServiceCommand>
{
    private readonly IOrderWriteAccess _access;
    private readonly IClock _clock;

    public StartServiceCommandHandler(IOrderWriteAccess access, IClock clock)
    {
        _access = access;
        _clock = clock;
    }

    public Task Handle(StartServiceCommand command, CancellationToken cancellationToken) =>
        _access.ChangeAsync(command.OrderId, order => order.Start(_clock.Now), cancellationToken);
}
