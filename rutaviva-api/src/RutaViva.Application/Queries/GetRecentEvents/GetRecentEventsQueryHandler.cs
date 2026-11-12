using RutaViva.Application.Abstractions.Messaging;
using RutaViva.Application.Ports;

namespace RutaViva.Application.Queries.GetRecentEvents;

internal sealed class GetRecentEventsQueryHandler : IQueryHandler<GetRecentEventsQuery, IReadOnlyList<RecordedEvent>>
{
    private readonly IEventLog _log;

    public GetRecentEventsQueryHandler(IEventLog log)
    {
        _log = log;
    }

    public Task<IReadOnlyList<RecordedEvent>> Handle(GetRecentEventsQuery query, CancellationToken cancellationToken) =>
        Task.FromResult(_log.NewestFirst());
}
