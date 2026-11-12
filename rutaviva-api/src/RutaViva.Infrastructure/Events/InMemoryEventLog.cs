using RutaViva.Application.Ports;
using RutaViva.Domain.DomainEvents.ServiceOrders;
using RutaViva.Domain.DomainEvents.Vehicles;
using RutaViva.Domain.Interfaces;

namespace RutaViva.Infrastructure.Events;

public sealed class InMemoryEventLog : IEventPublisher, IEventLog
{
    private readonly object _gate = new();
    private readonly List<RecordedEvent> _events = [];
    private readonly IClock _clock;

    public InMemoryEventLog(IClock clock)
    {
        _clock = clock;
    }

    public Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken)
    {
        DateTimeOffset occurredAt = _clock.Now;
        lock (_gate)
        {
            foreach (IDomainEvent domainEvent in events)
            {
                _events.Add(Map(domainEvent, occurredAt));
            }
        }

        return Task.CompletedTask;
    }

    public IReadOnlyList<RecordedEvent> NewestFirst()
    {
        lock (_gate)
        {
            return _events.AsEnumerable().Reverse().ToArray();
        }
    }

    private static RecordedEvent Map(IDomainEvent domainEvent, DateTimeOffset occurredAt) => domainEvent switch
    {
        OdometerAdvanced advanced => new("OdometerAdvanced", "Odómetro actualizado", advanced.VehicleId.Value, null, null, occurredAt),
        RoutineBecameDueSoon dueSoon => new("RoutineBecameDueSoon", "Rutina por vencer", dueSoon.VehicleId.Value, dueSoon.RoutineId.Value, null, occurredAt),
        RoutineBecameOverdue overdue => new("RoutineBecameOverdue", "Rutina vencida", overdue.VehicleId.Value, overdue.RoutineId.Value, null, occurredAt),
        ServiceOrderOpened opened => new("ServiceOrderOpened", "Orden abierta", opened.VehicleId.Value, opened.RoutineId.Value, opened.OrderId.Value, occurredAt),
        QuoteSubmitted quote => new("QuoteSubmitted", "Cotización enviada", null, null, quote.OrderId.Value, occurredAt),
        ServiceOrderApproved approved => new("ServiceOrderApproved", "Orden aprobada", null, null, approved.OrderId.Value, occurredAt),
        ServiceOrderRejected rejected => new("ServiceOrderRejected", "Cotización rechazada", null, null, rejected.OrderId.Value, occurredAt),
        ServiceOrderCancelled cancelled => new("ServiceOrderCancelled", "Orden cancelada", null, null, cancelled.OrderId.Value, occurredAt),
        ServiceStarted started => new("ServiceStarted", "Servicio iniciado", null, null, started.OrderId.Value, occurredAt),
        ServiceCompleted completed => new("ServiceCompleted", "Servicio completado", completed.VehicleId.Value, completed.RoutineId.Value, completed.OrderId.Value, occurredAt),
        _ => new(domainEvent.GetType().Name, domainEvent.GetType().Name, null, null, null, occurredAt)
    };
}
