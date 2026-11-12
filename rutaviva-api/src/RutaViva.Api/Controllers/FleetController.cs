using MediatR;
using Microsoft.AspNetCore.Mvc;
using RutaViva.Application.Queries.GetFleetSummary;

namespace RutaViva.Api.Controllers;

/// <summary>Reads the current client's fleet.</summary>
[ApiController]
[Route("api/fleet")]
[Tags("Client")]
[Produces("application/json")]
public sealed class FleetController : ControllerBase
{
    private readonly ISender _sender;

    public FleetController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Lists every vehicle of the current client with its routine status.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FleetVehicleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FleetVehicleDto>>> GetFleet(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetFleetSummaryQuery(), cancellationToken));
}
