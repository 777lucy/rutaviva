using MediatR;
using RutaViva.Application.Commands.Shared;
using RutaViva.Application.Ports;
using RutaViva.Domain.Aggregates.ServiceOrders;
using RutaViva.Domain.DomainEvents.Vehicles;
using RutaViva.Domain.Policies;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Application.Events;

internal sealed class RoutineBecameOverdueHandler : INotificationHandler<RoutineBecameOverdue>
{
    private readonly IServiceOrderRepository _ordersRepository;
    private readonly IClock _clock;
    private readonly IDomainEventFlusher _eventsFlusher;

    public RoutineBecameOverdueHandler(
        IServiceOrderRepository ordersRepository,
        IClock clock,
        IDomainEventFlusher eventsFlusher)
    {
        _ordersRepository = ordersRepository;
        _clock = clock;
        _eventsFlusher = eventsFlusher;
    }

    public async Task Handle(RoutineBecameOverdue notification, CancellationToken cancellationToken)
    {
        bool hasOpenOrder = await _ordersRepository.HasOpenOrderAsync(notification.VehicleId, notification.RoutineId, cancellationToken);
        if (!OpenOrderPolicy.CanOpen(hasOpenOrder))
        {
            return;
        }

        ServiceOrder order = ServiceOrder.Open(
            ServiceOrderId.New(),
            notification.VehicleId,
            notification.RoutineId,
            notification.RoutineName,
            notification.WorkshopId,
            _clock.Now);
        await _ordersRepository.SaveAsync(order, cancellationToken);
        await _eventsFlusher.FlushAsync([order], cancellationToken);
    }
}
