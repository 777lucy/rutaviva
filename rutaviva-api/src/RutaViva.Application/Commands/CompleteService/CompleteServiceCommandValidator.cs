using FluentValidation;

namespace RutaViva.Application.Commands.CompleteService;

internal sealed class CompleteServiceCommandValidator : AbstractValidator<CompleteServiceCommand>
{
    public CompleteServiceCommandValidator()
    {
        RuleFor(command => command.Kilometers)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Los kilómetros no pueden ser negativos.");

        RuleFor(command => command.PerformedOn)
            .NotNull()
            .WithMessage("La fecha del servicio es obligatoria.");
    }
}
