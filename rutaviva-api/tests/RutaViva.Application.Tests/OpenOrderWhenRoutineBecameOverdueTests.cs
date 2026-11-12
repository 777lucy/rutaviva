using RutaViva.Application.Commands.Shared;
using RutaViva.Application.Events;
using RutaViva.Application.Ports;
using RutaViva.Domain.DomainEvents.ServiceOrders;
using RutaViva.Domain.DomainEvents.Vehicles;
using RutaViva.Domain.Interfaces;
using RutaViva.Domain.ValueObjects.Identifiers;
using RutaViva.Infrastructure.Persistence;

namespace RutaViva.Application.Tests;

public sealed class OpenOrderWhenRoutineBecameOverdueTests
{
    [Fact]
    public async Task First_overdue_saves_one_order_and_a_second_saves_nothing()
    {
        InMemoryServiceOrderRepository ordersRepository = new InMemoryServiceOrderRepository();
        RecordingPublisher publisher = new RecordingPublisher();
        DomainEventFlusher events = new DomainEventFlusher(publisher);
        FixedClock clock = new FixedClock();
        RoutineBecameOverdueHandler handler = new RoutineBecameOverdueHandler(ordersRepository, clock, events);
        RoutineBecameOverdue overdue = new RoutineBecameOverdue(
            VehicleId.New(),
            RoutineId.New(),
            "Aceite",
            WorkshopId.New());

        await handler.Handle(overdue, CancellationToken.None);

        Assert.Single(await ordersRepository.ListByVehicleAsync(overdue.VehicleId, CancellationToken.None));
        Assert.Single(publisher.Events.OfType<ServiceOrderOpened>());
        Assert.All(
            await ordersRepository.ListByVehicleAsync(overdue.VehicleId, CancellationToken.None),
            order => Assert.Empty(order.DomainEvents));

        await handler.Handle(overdue, CancellationToken.None);

        Assert.Single(await ordersRepository.ListByVehicleAsync(overdue.VehicleId, CancellationToken.None));
        Assert.Single(publisher.Events.OfType<ServiceOrderOpened>());
    }

    private sealed class RecordingPublisher : IEventPublisher
    {
        public List<IDomainEvent> Events { get; } = [];

        public Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken)
        {
            Events.AddRange(events);
            return Task.CompletedTask;
        }
    }

    private sealed class FixedClock : IClock
    {
        public DateOnly Today { get; } = new(2026, 3, 1);

        public DateTimeOffset Now { get; } = new(2026, 3, 1, 12, 0, 0, TimeSpan.FromHours(-5));
    }
}
