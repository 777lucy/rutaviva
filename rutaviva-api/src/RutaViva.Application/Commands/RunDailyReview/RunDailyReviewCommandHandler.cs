using RutaViva.Application.Abstractions.Messaging;
using RutaViva.Application.Events;
using RutaViva.Application.Ports;
using RutaViva.Domain.Aggregates.Vehicles;
using RutaViva.Domain.Primitives;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Application.Commands.RunDailyReview;

internal sealed class RunDailyReviewCommandHandler
    : ICommandHandler<RunDailyReviewCommand, IReadOnlyList<RoutineChange>>
{
    private readonly IVehicleRepository _vehiclesRepository;
    private readonly IVehicleLock _lock;
    private readonly IClock _clock;
    private readonly ICurrentClient _client;
    private readonly IDomainEventDispatcher _events;

    public RunDailyReviewCommandHandler(
        IVehicleRepository vehiclesRepository,
        IVehicleLock vehicleLock,
        IClock clock,
        ICurrentClient client,
        IDomainEventDispatcher events)
    {
        _vehiclesRepository = vehiclesRepository;
        _lock = vehicleLock;
        _clock = clock;
        _client = client;
        _events = events;
    }

    public async Task<IReadOnlyList<RoutineChange>> Handle(RunDailyReviewCommand command, CancellationToken cancellationToken)
    {
        DateOnly reviewDate = command.Date ?? _clock.Today;
        List<RoutineChange> routineChanges = new List<RoutineChange>();
        foreach (Vehicle listedVehicle in await _vehiclesRepository.ListByClientAsync(_client.Id, cancellationToken))
        {
            await using (await _lock.AcquireAsync(listedVehicle.Id, cancellationToken))
            {
                Vehicle vehicle = await _vehiclesRepository.GetAsync(listedVehicle.Id, cancellationToken) ?? listedVehicle;
                routineChanges.AddRange(Review(vehicle, reviewDate));
                await _vehiclesRepository.SaveAsync(vehicle, cancellationToken);
                await _events.DispatchAsync(vehicle, cancellationToken);
            }
        }

        return routineChanges;
    }

    private static IEnumerable<RoutineChange> Review(Vehicle vehicle, DateOnly reviewDate)
    {
        Dictionary<RoutineId, RoutineStatus> statusBeforeReview = vehicle.Routines.ToDictionary(routine => routine.Id, routine => routine.Status);
        vehicle.RunDailyReview(reviewDate);
        return vehicle.Routines
            .Where(routine => statusBeforeReview[routine.Id] != routine.Status)
            .Select(routine => new RoutineChange(vehicle.Id.Value, routine.Id.Value, routine.Status.ToString()))
            .ToArray();
    }
}
