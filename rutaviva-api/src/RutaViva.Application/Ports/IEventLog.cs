namespace RutaViva.Application.Ports;

public interface IEventLog
{
    IReadOnlyList<RecordedEvent> NewestFirst();
}
