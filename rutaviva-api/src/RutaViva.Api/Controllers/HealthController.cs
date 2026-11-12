using Microsoft.AspNetCore.Mvc;

namespace RutaViva.Api.Controllers;

/// <summary>Process health.</summary>
[ApiController]
[Route("health")]
[Tags("System")]
[Produces("application/json")]
public sealed class HealthController : ControllerBase
{
    /// <summary>Returns ok when the process is running.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get() => Ok(new { status = "ok" });
}
