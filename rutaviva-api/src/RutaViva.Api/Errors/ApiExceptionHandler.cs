using Microsoft.AspNetCore.Diagnostics;
using RutaViva.Application.Errors;

namespace RutaViva.Api.Errors;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        int status = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            _ => 0
        };

        if (status == 0)
        {
            return false;
        }

        await Results.Problem(title: exception.Message, statusCode: status).ExecuteAsync(httpContext);
        return true;
    }
}
