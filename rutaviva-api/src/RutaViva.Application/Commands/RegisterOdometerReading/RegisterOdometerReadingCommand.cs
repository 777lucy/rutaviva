using RutaViva.Application.Abstractions.Messaging;

namespace RutaViva.Application.Commands.RegisterOdometerReading;

public sealed record RegisterOdometerReadingCommand(Guid VehicleId, int Kilometers) : ICommand<ReadingOutcome>;
