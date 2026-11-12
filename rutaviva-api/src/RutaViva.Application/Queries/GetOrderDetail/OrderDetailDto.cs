namespace RutaViva.Application.Queries.GetOrderDetail;

public sealed record OrderDetailDto(
    Guid Id,
    Guid VehicleId,
    string Plate,
    string RoutineName,
    string WorkshopName,
    string Status,
    decimal? QuoteAmount,
    string? CancellationReason,
    int? CompletionKilometers,
    DateOnly? CompletionDate,
    IReadOnlyList<OrderHistoryItemDto> History);
