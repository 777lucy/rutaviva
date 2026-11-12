namespace RutaViva.Api.Contracts;

/// <summary>Optional date for the daily review.</summary>
/// <param name="Date">Review date. Null uses today in America/Bogota.</param>
public sealed record DailyReviewRequest(DateOnly? Date);
