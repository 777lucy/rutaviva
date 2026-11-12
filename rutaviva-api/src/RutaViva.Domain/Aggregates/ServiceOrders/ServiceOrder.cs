using RutaViva.Domain.Aggregates;
using RutaViva.Domain.DomainEvents.ServiceOrders;
using RutaViva.Domain.Exceptions;
using RutaViva.Domain.Policies;
using RutaViva.Domain.Primitives;
using RutaViva.Domain.ValueObjects;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Domain.Aggregates.ServiceOrders;

public sealed class ServiceOrder : AggregateRoot
{
    private ServiceOrder(
        ServiceOrderId id,
        VehicleId vehicleId,
        RoutineId routineId,
        string routineName,
        WorkshopId workshopId,
        DateTimeOffset openedAt)
    {
        Id = id;
        VehicleId = vehicleId;
        RoutineId = routineId;
        RoutineName = routineName;
        WorkshopId = workshopId;
        OpenedAt = openedAt;
        Status = ServiceOrderStatus.Created;
    }

    public ServiceOrderId Id { get; }

    public VehicleId VehicleId { get; }

    public RoutineId RoutineId { get; }

    public string RoutineName { get; }

    public WorkshopId WorkshopId { get; }

    public ServiceOrderStatus Status { get; private set; }

    public Quote? Quote { get; private set; }

    public ServiceCompletion? Completion { get; private set; }

    public Cancellation? Cancellation { get; private set; }

    public DateTimeOffset OpenedAt { get; }

    public bool IsOpen => Status is not ServiceOrderStatus.Completed and not ServiceOrderStatus.Cancelled;

    public static ServiceOrder Open(
        ServiceOrderId id,
        VehicleId vehicleId,
        RoutineId routineId,
        string routineName,
        WorkshopId workshopId,
        DateTimeOffset openedAt)
    {
        ServiceOrder order = new ServiceOrder(id, vehicleId, routineId, routineName, workshopId, openedAt);
        order.Raise(new ServiceOrderOpened(id, vehicleId, routineId, workshopId));
        return order;
    }

    public void SubmitQuote(Money amount, DateTimeOffset occurredAt)
    {
        EnsureStatus(ServiceOrderStatus.Created);
        Quote = new Quote(amount, occurredAt);
        if (QuoteApprovalPolicy.RequiresManualApproval(amount))
        {
            Status = ServiceOrderStatus.Quoted;
            Raise(new QuoteSubmitted(Id, amount.Amount));
            return;
        }

        Status = ServiceOrderStatus.Approved;
        Raise(new ServiceOrderApproved(Id, ApprovalSource.Auto));
    }

    public void Approve(DateTimeOffset occurredAt)
    {
        EnsureStatus(ServiceOrderStatus.Quoted);
        Status = ServiceOrderStatus.Approved;
        Raise(new ServiceOrderApproved(Id, ApprovalSource.ByClient));
        _ = occurredAt;
    }

    public void RejectQuote(DateTimeOffset occurredAt)
    {
        EnsureStatus(ServiceOrderStatus.Quoted);
        Status = ServiceOrderStatus.Cancelled;
        Cancellation = new Cancellation(CancellationReason.QuoteRejected, occurredAt);
        Raise(new ServiceOrderRejected(Id));
    }

    public void Cancel(DateTimeOffset occurredAt)
    {
        if (Status is ServiceOrderStatus.InProgress or ServiceOrderStatus.Completed or ServiceOrderStatus.Cancelled)
        {
            throw new DomainException("order.transition", "La orden no se puede cancelar cuando el servicio ya inició.");
        }

        EnsureStatus(ServiceOrderStatus.Created, ServiceOrderStatus.Quoted, ServiceOrderStatus.Approved);
        Status = ServiceOrderStatus.Cancelled;
        Cancellation = new Cancellation(CancellationReason.CancelledByClient, occurredAt);
        Raise(new ServiceOrderCancelled(Id));
    }

    public void Start(DateTimeOffset occurredAt)
    {
        EnsureStatus(ServiceOrderStatus.Approved);
        Status = ServiceOrderStatus.InProgress;
        Raise(new ServiceStarted(Id));
        _ = occurredAt;
    }

    public void Complete(Kilometers kilometers, DateOnly? performedOn, DateTimeOffset occurredAt)
    {
        EnsureStatus(ServiceOrderStatus.InProgress);
        if (performedOn is null)
        {
            throw new DomainException("completion.date", "La fecha del servicio es obligatoria.");
        }

        ServiceCompletion completion = new ServiceCompletion(kilometers, performedOn.Value);
        Completion = completion;
        Status = ServiceOrderStatus.Completed;
        Raise(new ServiceCompleted(Id, VehicleId, RoutineId, completion));
        _ = occurredAt;
    }

    private void EnsureStatus(params ServiceOrderStatus[] allowed)
    {
        if (!allowed.Contains(Status))
        {
            throw new DomainException("order.transition", "La orden no admite esta acción en su estado actual.");
        }
    }
}
