namespace RutaViva.Api.Contracts;

/// <summary>A workshop quote.</summary>
/// <param name="Amount">Amount in COP. Above 1,500,000 requires client approval.</param>
public sealed record QuoteRequest(decimal Amount);
