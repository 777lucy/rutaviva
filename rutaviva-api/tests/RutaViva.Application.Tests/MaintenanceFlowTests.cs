using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RutaViva.Application.Commands.ApproveQuote;
using RutaViva.Application.Commands.CancelOrder;
using RutaViva.Application.Commands.CompleteService;
using RutaViva.Application.Commands.RegisterOdometerReading;
using RutaViva.Application.Commands.RejectQuote;
using RutaViva.Application.Commands.RunDailyReview;
using RutaViva.Application.Commands.StartService;
using RutaViva.Application.Commands.SubmitQuote;
using RutaViva.Application.Errors;
using RutaViva.Application.Ports;
using RutaViva.Application.Queries.GetFleetSummary;
using RutaViva.Application.Queries.GetOrderDetail;
using RutaViva.Application.Queries.GetRecentEvents;
using RutaViva.Application.Queries.GetVehicleDetail;
using RutaViva.Domain.Aggregates.ServiceOrders;
using RutaViva.Domain.Aggregates.Vehicles;
using RutaViva.Domain.Primitives;
using RutaViva.Domain.ValueObjects;
using RutaViva.Domain.ValueObjects.Identifiers;
using RutaViva.Infrastructure;

namespace RutaViva.Application.Tests;

public sealed class MaintenanceFlowTests : IDisposable
{
    private readonly FakeClient _client = new();
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;
    private readonly ISender _sender;
    private readonly IVehicleRepository _vehiclesRepository;
    private readonly IServiceOrderRepository _ordersRepository;

    public MaintenanceFlowTests()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddApplication();
        services.AddInfrastructure();
        services.AddSingleton<IClock>(new FakeClock());
        services.AddSingleton<ICurrentClient>(_client);
        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateScope();
        _sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        _vehiclesRepository = _scope.ServiceProvider.GetRequiredService<IVehicleRepository>();
        _ordersRepository = _scope.ServiceProvider.GetRequiredService<IServiceOrderRepository>();
    }

    [Fact]
    public async Task Accepted_reading_opens_one_order_and_a_later_reading_does_not_open_another()
    {
        Vehicle vehicle = await OilChangeAsync(new Kilometers(0));

        ReadingOutcome accepted = await ReadAsync(vehicle, 10_000);
        ReadingOutcome laterReading = await ReadAsync(vehicle, 10_500);

        Assert.Equal(ReadingOutcome.Accepted, accepted);
        Assert.Equal(ReadingOutcome.Accepted, laterReading);
        Assert.Single(await _ordersRepository.ListByVehicleAsync(vehicle.Id, CancellationToken.None));
        IReadOnlyList<RecordedEvent> events = await _sender.Send(new GetRecentEventsQuery());
        Assert.Contains(events, recordedEvent => recordedEvent.Name == "RoutineBecameOverdue");
        Assert.Contains(events, recordedEvent => recordedEvent.Name == "ServiceOrderOpened");
    }

    [Fact]
    public async Task Lower_reading_is_ignored_without_events()
    {
        Vehicle vehicle = await OilChangeAsync(new Kilometers(20_000));

        ReadingOutcome outcome = await ReadAsync(vehicle, 19_000);

        Assert.Equal(ReadingOutcome.Ignored, outcome);
        Assert.Equal(20_000, (await _vehiclesRepository.GetAsync(vehicle.Id, CancellationToken.None))!.Odometer.Value);
        Assert.Empty(await _sender.Send(new GetRecentEventsQuery()));
    }

    [Fact]
    public async Task Reading_for_an_unknown_vehicle_is_not_found()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sender.Send(new RegisterOdometerReadingCommand(Guid.NewGuid(), 1_000)));
    }

    [Fact]
    public async Task Negative_reading_is_a_validation_error()
    {
        Vehicle vehicle = await OilChangeAsync(new Kilometers(1_000));

        await Assert.ThrowsAsync<ValidationException>(() => ReadAsync(vehicle, -1));
    }

    [Fact]
    public async Task Daily_review_marks_due_soon_and_overdue_opens_an_order()
    {
        Vehicle vehicle = await TimeRoutineAsync(new DateOnly(2026, 1, 1));

        IReadOnlyList<RoutineChange> dueSoonChanges = await _sender.Send(new RunDailyReviewCommand(new DateOnly(2026, 6, 13)));
        Assert.Equal("DueSoon", dueSoonChanges.Single().Status);
        Assert.Contains(await _sender.Send(new GetRecentEventsQuery()), recordedEvent => recordedEvent.Name == "RoutineBecameDueSoon");

        IReadOnlyList<RoutineChange> overdueChanges = await _sender.Send(new RunDailyReviewCommand(new DateOnly(2026, 7, 1)));
        Assert.Equal("Overdue", overdueChanges.Single().Status);
        Assert.Single(await _ordersRepository.ListByVehicleAsync(vehicle.Id, CancellationToken.None));
    }

    [Fact]
    public async Task Reading_does_not_change_time_status()
    {
        Vehicle vehicle = await TimeRoutineAsync(new DateOnly(2026, 1, 1));

        await ReadAsync(vehicle, 30_000);

        Assert.Equal(RoutineStatus.UpToDate, (await _vehiclesRepository.GetAsync(vehicle.Id, CancellationToken.None))!.Routines.Single().Status);
    }

    [Fact]
    public async Task Quote_above_the_threshold_waits_and_the_client_can_approve()
    {
        ServiceOrder order = await OverdueOrderAsync();

        await _sender.Send(new SubmitQuoteCommand(order.Id.Value, 1_800_000m));
        Assert.Equal(ServiceOrderStatus.Quoted, await StatusAsync(order));

        await _sender.Send(new ApproveQuoteCommand(order.Id.Value));
        Assert.Equal(ServiceOrderStatus.Approved, await StatusAsync(order));
    }

    [Fact]
    public async Task Quote_at_the_threshold_is_approved_automatically()
    {
        ServiceOrder order = await OverdueOrderAsync();

        await _sender.Send(new SubmitQuoteCommand(order.Id.Value, 1_500_000m));

        Assert.Equal(ServiceOrderStatus.Approved, await StatusAsync(order));
    }

    [Fact]
    public async Task Reject_leaves_the_routine_overdue_and_a_later_reading_opens_no_replacement()
    {
        ServiceOrder order = await OverdueOrderAsync();
        await _sender.Send(new SubmitQuoteCommand(order.Id.Value, 2_000_000m));

        await _sender.Send(new RejectQuoteCommand(order.Id.Value));
        Vehicle vehicle = (await _vehiclesRepository.GetAsync(order.VehicleId, CancellationToken.None))!;
        await ReadAsync(vehicle, 12_000);

        Assert.Equal(RoutineStatus.Overdue, vehicle.Routines.Single().Status);
        IReadOnlyList<ServiceOrder> orders = await _ordersRepository.ListByVehicleAsync(vehicle.Id, CancellationToken.None);
        Assert.Equal(CancellationReason.QuoteRejected, Assert.Single(orders).Cancellation!.Reason);
    }

    [Fact]
    public async Task Cancel_is_allowed_before_execution_and_refused_after_start()
    {
        ServiceOrder cancelled = await OverdueOrderAsync();
        await _sender.Send(new CancelOrderCommand(cancelled.Id.Value));
        Assert.Equal(ServiceOrderStatus.Cancelled, await StatusAsync(cancelled));

        ServiceOrder running = await ApprovedOrderAsync();
        await _sender.Send(new StartServiceCommand(running.Id.Value));

        await Assert.ThrowsAsync<ConflictException>(() => _sender.Send(new CancelOrderCommand(running.Id.Value)));
        Assert.Equal(ServiceOrderStatus.InProgress, await StatusAsync(running));
    }

    [Fact]
    public async Task Start_before_approval_is_a_conflict()
    {
        ServiceOrder order = await OverdueOrderAsync();

        await Assert.ThrowsAsync<ConflictException>(() => _sender.Send(new StartServiceCommand(order.Id.Value)));
        Assert.Equal(ServiceOrderStatus.Created, await StatusAsync(order));
    }

    [Fact]
    public async Task Complete_restarts_the_routine_from_the_reported_values()
    {
        ServiceOrder order = await ApprovedOrderAsync();
        await _sender.Send(new StartServiceCommand(order.Id.Value));

        await _sender.Send(new CompleteServiceCommand(order.Id.Value, 12_000, new DateOnly(2026, 7, 2)));

        MaintenanceRoutine routine = (await _vehiclesRepository.GetAsync(order.VehicleId, CancellationToken.None))!.Routines.Single();
        Assert.Equal(ServiceOrderStatus.Completed, await StatusAsync(order));
        Assert.Equal(RoutineStatus.UpToDate, routine.Status);
        Assert.Equal(new ServiceBaseline(new Kilometers(12_000), new DateOnly(2026, 7, 2)), routine.Baseline);
    }

    [Fact]
    public async Task Complete_without_a_date_is_a_validation_error()
    {
        ServiceOrder order = await ApprovedOrderAsync();
        await _sender.Send(new StartServiceCommand(order.Id.Value));

        await Assert.ThrowsAsync<ValidationException>(() =>
            _sender.Send(new CompleteServiceCommand(order.Id.Value, 12_000, null)));
        Assert.Equal(ServiceOrderStatus.InProgress, await StatusAsync(order));
    }

    [Fact]
    public async Task Fleet_and_vehicle_queries_show_the_current_client_only()
    {
        Vehicle vehicle = await OilChangeAsync(new Kilometers(1_000));
        Vehicle foreign = await ForeignVehicleAsync();

        IReadOnlyList<FleetVehicleDto> fleet = await _sender.Send(new GetFleetSummaryQuery());
        VehicleDetailDto detail = await _sender.Send(new GetVehicleDetailQuery(vehicle.Id.Value));

        Assert.Equal(vehicle.Id.Value, Assert.Single(fleet).Id);
        Assert.Equal("Cada 10.000 km", detail.Routines.Single().Interval);
        await Assert.ThrowsAsync<NotFoundException>(() => _sender.Send(new GetVehicleDetailQuery(foreign.Id.Value)));
    }

    [Fact]
    public async Task Order_detail_shows_the_quote_and_hides_foreign_ordersRepository()
    {
        ServiceOrder order = await OverdueOrderAsync();
        await _sender.Send(new SubmitQuoteCommand(order.Id.Value, 1_800_000m));
        Vehicle foreign = await ForeignVehicleAsync();
        ServiceOrder foreignOrder = ServiceOrder.Open(
            ServiceOrderId.New(),
            foreign.Id,
            foreign.Routines.Single().Id,
            "Aceite",
            foreign.AssignedWorkshopId,
            DateTimeOffset.UnixEpoch);
        await _ordersRepository.SaveAsync(foreignOrder, CancellationToken.None);

        OrderDetailDto detail = await _sender.Send(new GetOrderDetailQuery(order.Id.Value));

        Assert.Equal("Quoted", detail.Status);
        Assert.Equal(1_800_000m, detail.QuoteAmount);
        Assert.True(detail.History.Single(step => step.Name == "Cotizada").Done);
        await Assert.ThrowsAsync<NotFoundException>(() => _sender.Send(new GetOrderDetailQuery(foreignOrder.Id.Value)));
    }

    public void Dispose()
    {
        _scope.Dispose();
        _provider.Dispose();
    }

    private Task<ReadingOutcome> ReadAsync(Vehicle vehicle, int kilometers) =>
        _sender.Send(new RegisterOdometerReadingCommand(vehicle.Id.Value, kilometers));

    private async Task<ServiceOrderStatus> StatusAsync(ServiceOrder order) =>
        (await _ordersRepository.GetAsync(order.Id, CancellationToken.None))!.Status;

    private async Task<ServiceOrder> OverdueOrderAsync()
    {
        Vehicle vehicle = await OilChangeAsync(new Kilometers(0));
        await ReadAsync(vehicle, 10_000);
        return (await _ordersRepository.ListByVehicleAsync(vehicle.Id, CancellationToken.None)).Single();
    }

    private async Task<ServiceOrder> ApprovedOrderAsync()
    {
        ServiceOrder order = await OverdueOrderAsync();
        await _sender.Send(new SubmitQuoteCommand(order.Id.Value, 1_000_000m));
        return order;
    }

    private Task<Vehicle> OilChangeAsync(Kilometers odometer) =>
        SaveVehicleAsync(_client.Id, odometer, MaintenanceInterval.Create(new Kilometers(10_000), null), new DateOnly(2026, 1, 1));

    private Task<Vehicle> TimeRoutineAsync(DateOnly baseline) =>
        SaveVehicleAsync(_client.Id, new Kilometers(1_000), MaintenanceInterval.Create(null, 6), baseline);

    private Task<Vehicle> ForeignVehicleAsync() =>
        SaveVehicleAsync(FleetClientId.New(), new Kilometers(1_000), MaintenanceInterval.Create(new Kilometers(10_000), null), new DateOnly(2026, 1, 1));

    private async Task<Vehicle> SaveVehicleAsync(
        FleetClientId clientId,
        Kilometers odometer,
        MaintenanceInterval interval,
        DateOnly baselineDate)
    {
        Vehicle vehicle = Vehicle.Create(VehicleId.New(), clientId, WorkshopId.New(), "ABC-123", "Camión", odometer);
        vehicle.AddRoutine(RoutineId.New(), "Aceite", interval, new ServiceBaseline(new Kilometers(0), baselineDate));
        await _vehiclesRepository.SaveAsync(vehicle, CancellationToken.None);
        return vehicle;
    }

    private sealed class FakeClock : IClock
    {
        public DateOnly Today { get; } = new(2026, 3, 1);

        public DateTimeOffset Now { get; } = new(2026, 3, 1, 12, 0, 0, TimeSpan.FromHours(-5));
    }

    private sealed class FakeClient : ICurrentClient
    {
        public FleetClientId Id { get; } = FleetClientId.New();
    }
}
