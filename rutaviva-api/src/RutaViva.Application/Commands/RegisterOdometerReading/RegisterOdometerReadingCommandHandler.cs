using RutaViva.Application.Abstractions.Messaging;
using RutaViva.Application.Errors;
using RutaViva.Application.Events;
using RutaViva.Application.Ports;
using RutaViva.Domain.Aggregates.Vehicles;
using RutaViva.Domain.Primitives;
using RutaViva.Domain.ValueObjects;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Application.Commands.RegisterOdometerReading;

internal sealed class RegisterOdometerReadingCommandHandler
    : ICommandHandler<RegisterOdometerReadingCommand, ReadingOutcome>
{
    private readonly IVehicleRepository _vehiclesRepository;
    private readonly IVehicleLock _lock;
    private readonly IDomainEventDispatcher _events;

    public RegisterOdometerReadingCommandHandler(
        IVehicleRepository vehiclesRepository,
        IVehicleLock vehicleLock,
        IDomainEventDispatcher events)
    {
        _vehiclesRepository = vehiclesRepository;
        _lock = vehicleLock;
        _events = events;
    }

    public async Task<ReadingOutcome> Handle(RegisterOdometerReadingCommand command, CancellationToken cancellationToken)
    {
        VehicleId vehicleId = new VehicleId(command.VehicleId);
        await using (await _lock.AcquireAsync(vehicleId, cancellationToken))
        {
            Vehicle vehicle = await _vehiclesRepository.GetAsync(vehicleId, cancellationToken)
                ?? throw new NotFoundException("No se encontró el vehículo.");

            if (vehicle.RegisterOdometerReading(new Kilometers(command.Kilometers)) == ReadingResult.Ignored)
            {
                return ReadingOutcome.Ignored;
            }

            await _vehiclesRepository.SaveAsync(vehicle, cancellationToken);
            await _events.DispatchAsync(vehicle, cancellationToken);
            return ReadingOutcome.Accepted;
        }
    }
}
