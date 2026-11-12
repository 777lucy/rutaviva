using RutaViva.Application.Abstractions.Messaging;
using RutaViva.Application.Commands.Shared;
using RutaViva.Application.Ports;
using RutaViva.Domain.Aggregates.ServiceOrders;
using RutaViva.Domain.Aggregates.Vehicles;
using RutaViva.Domain.ValueObjects;

namespace RutaViva.Application.Commands.CompleteService;

internal sealed class CompleteServiceCommandHandler : ICommandHandler<CompleteServiceCommand>
{
    private readonly IOrderWriteAccess _access;
    private readonly IClock _clock;

    public CompleteServiceCommandHandler(IOrderWriteAccess access, IClock clock)
    {
        _access = access;
        _clock = clock;
    }

    public async Task Handle(CompleteServiceCommand command, CancellationToken cancellationToken)
    {
        await using (await _access.LockAsync(command.OrderId, cancellationToken))
        {
            (ServiceOrder order, Vehicle vehicle) = await _access.LoadAsync(command.OrderId, cancellationToken);
            order.Complete(new Kilometers(command.Kilometers), command.PerformedOn, _clock.Now);
            vehicle.RestartRoutine(order.RoutineId, order.Completion!);
            await _access.SaveAsync(order, vehicle, cancellationToken);
        }
    }
}
