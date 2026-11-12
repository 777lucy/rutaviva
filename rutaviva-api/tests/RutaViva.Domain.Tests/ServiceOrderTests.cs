using RutaViva.Domain.Aggregates.ServiceOrders;
using RutaViva.Domain.DomainEvents.ServiceOrders;
using RutaViva.Domain.Exceptions;
using RutaViva.Domain.Policies;
using RutaViva.Domain.Primitives;
using RutaViva.Domain.ValueObjects;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Domain.Tests;

public sealed class ServiceOrderTests
{
    private static readonly DateTimeOffset At = new(2026, 11, 10, 15, 0, 0, TimeSpan.FromHours(-5));

    [Fact]
    public void Quote_above_the_threshold_waits_and_the_threshold_is_approved()
    {
        ServiceOrder quoteAboveThreshold = Open();
        quoteAboveThreshold.SubmitQuote(new Money(1_800_000m), At);
        Assert.Equal(ServiceOrderStatus.Quoted, quoteAboveThreshold.Status);

        ServiceOrder quoteAtThreshold = Open();
        quoteAtThreshold.SubmitQuote(new Money(1_500_000m), At);
        Assert.Equal(ServiceOrderStatus.Approved, quoteAtThreshold.Status);
        Assert.Contains(quoteAtThreshold.DomainEvents, domainEvent => domainEvent is ServiceOrderApproved approvedEvent && approvedEvent.Source == ApprovalSource.Auto);

        ServiceOrder quoteBelowThreshold = Open();
        quoteBelowThreshold.SubmitQuote(new Money(1_000_000m), At);
        Assert.Equal(ServiceOrderStatus.Approved, quoteBelowThreshold.Status);
    }

    [Fact]
    public void Client_can_approve_or_reject_only_from_quoted()
    {
        ServiceOrder order = Quoted();

        order.Approve(At);

        Assert.Equal(ServiceOrderStatus.Approved, order.Status);

        ServiceOrder rejected = Quoted();
        rejected.RejectQuote(At);
        Assert.Equal(ServiceOrderStatus.Cancelled, rejected.Status);
        Assert.Equal(CancellationReason.QuoteRejected, rejected.Cancellation!.Reason);
    }

    [Fact]
    public void Client_can_cancel_before_execution_and_not_after_start()
    {
        ServiceOrder created = Open();
        created.Cancel(At);
        Assert.Equal(CancellationReason.CancelledByClient, created.Cancellation!.Reason);

        ServiceOrder approved = Approved();
        approved.Cancel(At);
        Assert.Equal(ServiceOrderStatus.Cancelled, approved.Status);

        ServiceOrder running = Approved();
        running.Start(At);
        DomainException domainException = Assert.Throws<DomainException>(() => running.Cancel(At));
        Assert.Equal(ServiceOrderStatus.InProgress, running.Status);
        Assert.False(string.IsNullOrWhiteSpace(domainException.Message));
    }

    [Fact]
    public void Start_requires_approval_and_complete_requires_a_date()
    {
        ServiceOrder quoted = Quoted();
        Assert.Throws<DomainException>(() => quoted.Start(At));
        Assert.Equal(ServiceOrderStatus.Quoted, quoted.Status);

        ServiceOrder running = Approved();
        running.Start(At);
        Assert.Throws<DomainException>(() => running.Complete(new Kilometers(61_000), null, At));
        Assert.Equal(ServiceOrderStatus.InProgress, running.Status);

        running.Complete(new Kilometers(61_000), new DateOnly(2026, 11, 10), At);
        Assert.Equal(ServiceOrderStatus.Completed, running.Status);
        Assert.False(running.IsOpen);
    }

    [Fact]
    public void Open_order_policy_refuses_a_second_open_order()
    {
        Assert.True(OpenOrderPolicy.CanOpen(hasOpenOrder: false));
        Assert.False(OpenOrderPolicy.CanOpen(hasOpenOrder: true));
    }

    private static ServiceOrder Open() =>
        ServiceOrder.Open(ServiceOrderId.New(), VehicleId.New(), RoutineId.New(), "Frenos", WorkshopId.New(), At);

    private static ServiceOrder Quoted()
    {
        ServiceOrder order = Open();
        order.SubmitQuote(new Money(1_800_000m), At);
        return order;
    }

    private static ServiceOrder Approved()
    {
        ServiceOrder order = Open();
        order.SubmitQuote(new Money(1_500_000m), At);
        return order;
    }
}
