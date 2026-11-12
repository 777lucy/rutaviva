namespace RutaViva.Api.Contracts;

/// <summary>A telemetry reading for one vehicle.</summary>
/// <param name="VehicleId">Vehicle identifier.</param>
/// <param name="Kilometers">Absolute odometer value. Must be zero or greater.</param>
public sealed record OdometerReadingRequest(Guid VehicleId, int Kilometers);
