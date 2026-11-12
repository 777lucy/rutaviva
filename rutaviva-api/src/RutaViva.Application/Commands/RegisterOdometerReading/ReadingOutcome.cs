namespace RutaViva.Application.Commands.RegisterOdometerReading;

public sealed record ReadingOutcome(string Result)
{
    public static ReadingOutcome Accepted { get; } = new(nameof(Accepted));

    public static ReadingOutcome Ignored { get; } = new(nameof(Ignored));
}
