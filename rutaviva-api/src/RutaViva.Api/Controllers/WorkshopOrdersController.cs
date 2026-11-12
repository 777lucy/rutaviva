using MediatR;
using Microsoft.AspNetCore.Mvc;
using RutaViva.Api.Contracts;
using RutaViva.Application.Commands.CompleteService;
using RutaViva.Application.Commands.StartService;
using RutaViva.Application.Commands.SubmitQuote;

namespace RutaViva.Api.Controllers;

/// <summary>Workshop actions on a service order.</summary>
[ApiController]
[Route("api/workshop/orders/{id:guid}")]
[Tags("Workshop")]
[Produces("application/json")]
public sealed class WorkshopOrdersController : ControllerBase
{
    private readonly ISender _sender;

    public WorkshopOrdersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Submits a quote. Above 1,500,000 COP the order stays Quoted. At or below that amount it is approved immediately.</summary>
    /// <response code="400">The amount is negative.</response>
    /// <response code="409">The order is not Created.</response>
    [HttpPost("quote")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitQuote(Guid id, QuoteRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new SubmitQuoteCommand(id, request.Amount), cancellationToken);
        return Ok();
    }

    /// <summary>Starts the service. Allowed only from Approved.</summary>
    /// <response code="409">The order is not Approved.</response>
    [HttpPost("start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Start(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new StartServiceCommand(id), cancellationToken);
        return Ok();
    }

    /// <summary>Completes the service and restarts the routine from the reported kilometers and date.</summary>
    /// <response code="400">Kilometers are negative or the service date is missing.</response>
    /// <response code="409">The order is not InProgress.</response>
    [HttpPost("complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Complete(Guid id, CompleteServiceRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new CompleteServiceCommand(id, request.Kilometers, request.PerformedOn), cancellationToken);
        return Ok();
    }
}
