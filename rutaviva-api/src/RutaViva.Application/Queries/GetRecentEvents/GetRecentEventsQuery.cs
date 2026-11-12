using RutaViva.Application.Abstractions.Messaging;
using RutaViva.Application.Ports;

namespace RutaViva.Application.Queries.GetRecentEvents;

public sealed record GetRecentEventsQuery : IQuery<IReadOnlyList<RecordedEvent>>;
