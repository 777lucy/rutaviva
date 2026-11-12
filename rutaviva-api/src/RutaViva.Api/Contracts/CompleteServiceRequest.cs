namespace RutaViva.Api.Contracts;

/// <summary>The reported result of a finished service.</summary>
/// <param name="Kilometers">Kilometers reported by the workshop. A lower value does not decrease the odometer.</param>
/// <param name="PerformedOn">Calendar date of the service. Required.</param>
public sealed record CompleteServiceRequest(int Kilometers, DateOnly? PerformedOn);
