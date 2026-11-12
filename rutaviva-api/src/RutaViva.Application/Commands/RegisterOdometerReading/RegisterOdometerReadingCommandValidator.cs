using FluentValidation;

namespace RutaViva.Application.Commands.RegisterOdometerReading;

internal sealed class RegisterOdometerReadingCommandValidator : AbstractValidator<RegisterOdometerReadingCommand>
{
    public RegisterOdometerReadingCommandValidator()
    {
        RuleFor(command => command.Kilometers)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Los kilómetros no pueden ser negativos.");
    }
}
