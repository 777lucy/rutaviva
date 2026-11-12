using MediatR;
using Microsoft.AspNetCore.Mvc;
using RutaViva.Application.Queries.GetVehicleDetail;

namespace RutaViva.Api.Controllers;

/// <summary>Reads one vehicle of the current client.</summary>
[ApiController]
[Route("api/vehicles")]
[Tags("Client")]
[Produces("application/json")]
public sealed class VehiclesController : ControllerBase
{
    private readonly ISender _sender;

    public VehiclesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Returns the vehicle, each routine interval, and the open order when one exists.</summary>
    /// <response code="404">The vehicle does not exist or belongs to another client.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VehicleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleDetailDto>> GetVehicle(Guid id, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetVehicleDetailQuery(id), cancellationToken));
}
