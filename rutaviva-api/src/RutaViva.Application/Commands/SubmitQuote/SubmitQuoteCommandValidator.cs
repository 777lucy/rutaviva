using FluentValidation;

namespace RutaViva.Application.Commands.SubmitQuote;

internal sealed class SubmitQuoteCommandValidator : AbstractValidator<SubmitQuoteCommand>
{
    public SubmitQuoteCommandValidator()
    {
        RuleFor(command => command.Amount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El valor no puede ser negativo.");
    }
}
