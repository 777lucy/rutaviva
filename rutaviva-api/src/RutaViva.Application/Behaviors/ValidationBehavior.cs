using FluentValidation;
using FluentValidation.Results;
using MediatR;
using ValidationException = RutaViva.Application.Errors.ValidationException;

namespace RutaViva.Application.Behaviors;

// Las reglas de entrada viven en FluentValidation. El fallo sale como ValidationException
// para que la API responda 400 sin referenciar esa librería.
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        ValidationContext<TRequest> context = new(request);
        ValidationResult[] results = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        string[] messages = results
            .SelectMany(result => result.Errors)
            .Select(failure => failure.ErrorMessage)
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .Distinct()
            .ToArray();

        if (messages.Length > 0)
        {
            throw new ValidationException(string.Join(' ', messages));
        }

        return await next();
    }
}
