using MediatR;
using Microsoft.AspNetCore.Mvc;
using RutaViva.Api.Contracts;
using RutaViva.Application.Commands.RegisterOdometerReading;

namespace RutaViva.Api.Controllers;

/// <summary>Odometer readings from telemetry.</summary>
[ApiController]
[Route("api/telemetry")]
[Tags("Telemetry")]
[Produces("application/json")]
public sealed class TelemetryController : ControllerBase
{
    private readonly ISender _sender;

    public TelemetryController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Registers a reading. A value above the odometer is Accepted. An equal or lower value is Ignored and changes nothing.</summary>
    /// <response code="400">Kilometers are negative.</response>
    /// <response code="404">The vehicle does not exist.</response>
    [HttpPost("readings")]
    [ProducesResponseType(typeof(ReadingOutcome), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadingOutcome>> RegisterReading(
        OdometerReadingRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new RegisterOdometerReadingCommand(request.VehicleId, request.Kilometers), cancellationToken));
}
