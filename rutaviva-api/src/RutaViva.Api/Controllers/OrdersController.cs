using MediatR;
using Microsoft.AspNetCore.Mvc;
using RutaViva.Application.Commands.ApproveQuote;
using RutaViva.Application.Commands.CancelOrder;
using RutaViva.Application.Commands.RejectQuote;
using RutaViva.Application.Queries.GetOrderDetail;

namespace RutaViva.Api.Controllers;

/// <summary>Client actions on a service order.</summary>
[ApiController]
[Route("api/orders/{id:guid}")]
[Tags("Client")]
[Produces("application/json")]
public sealed class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Returns the order, its quote, and the history the order screen shows.</summary>
    /// <response code="404">The order does not exist or belongs to another client.</response>
    [HttpGet]
    [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetailDto>> GetOrder(Guid id, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetOrderDetailQuery(id), cancellationToken));

    /// <summary>Approves a quoted order. Allowed only while the order is Quoted.</summary>
    /// <response code="409">The order is not Quoted.</response>
    [HttpPost("approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new ApproveQuoteCommand(id), cancellationToken);
        return Ok();
    }

    /// <summary>Rejects a quoted order and cancels it. The routine stays overdue.</summary>
    /// <response code="409">The order is not Quoted.</response>
    [HttpPost("reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new RejectQuoteCommand(id), cancellationToken);
        return Ok();
    }

    /// <summary>Cancels the order before the workshop starts it.</summary>
    /// <response code="409">The service already started, or the order is already closed.</response>
    [HttpPost("cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new CancelOrderCommand(id), cancellationToken);
        return Ok();
    }
}
