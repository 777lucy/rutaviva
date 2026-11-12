using MediatR;
using RutaViva.Application.Errors;
using RutaViva.Domain.Exceptions;

namespace RutaViva.Application.Behaviors;

// La API no referencia el dominio: toda regla de negocio violada sale como conflicto (409).
public sealed class DomainExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (DomainException exception)
        {
            throw new ConflictException(exception.Message);
        }
    }
}
