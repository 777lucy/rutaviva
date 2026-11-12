namespace RutaViva.Application.Ports;

public interface IClock
{
    DateOnly Today { get; }

    DateTimeOffset Now { get; }
}
