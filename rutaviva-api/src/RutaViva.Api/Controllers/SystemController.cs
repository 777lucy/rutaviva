using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RutaViva.Api.Contracts;
using RutaViva.Application.Commands.RunDailyReview;
using RutaViva.Application.Ports;
using RutaViva.Application.Queries.GetRecentEvents;

namespace RutaViva.Api.Controllers;

/// <summary>Operations used by the simulation panel.</summary>
[ApiController]
[Route("api/system")]
[Tags("System")]
[Produces("application/json")]
public sealed class SystemController : ControllerBase
{
    private readonly ISender _sender;

    public SystemController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Runs the time review for the current client's vehicles. Omit the date to use today in America/Bogota.</summary>
    [HttpPost("daily-review")]
    [ProducesResponseType(typeof(IReadOnlyList<RoutineChange>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RoutineChange>>> RunDailyReview(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] DailyReviewRequest? request,
        CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new RunDailyReviewCommand(request?.Date), cancellationToken));

    /// <summary>Returns published domain events, newest first. Titles are in Spanish.</summary>
    [HttpGet("events")]
    [ProducesResponseType(typeof(IReadOnlyList<RecordedEvent>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RecordedEvent>>> GetEvents(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetRecentEventsQuery(), cancellationToken));
}
