using FluentValidation.Results;
using RutaViva.Application.Commands.CompleteService;
using RutaViva.Application.Commands.RegisterOdometerReading;
using RutaViva.Application.Commands.SubmitQuote;

namespace RutaViva.Application.Tests;

public sealed class CommandValidatorTests
{
    private readonly CompleteServiceCommandValidator _completeService = new();
    private readonly RegisterOdometerReadingCommandValidator _reading = new();
    private readonly SubmitQuoteCommandValidator _quote = new();

    [Fact]
    public void Complete_service_rejects_negative_kilometers()
    {
        ValidationResult result = _completeService.Validate(
            new CompleteServiceCommand(Guid.NewGuid(), -1, new DateOnly(2026, 7, 2)));

        Assert.Equal("Los kilómetros no pueden ser negativos.", Assert.Single(result.Errors).ErrorMessage);
    }

    [Fact]
    public void Complete_service_rejects_a_missing_date()
    {
        ValidationResult result = _completeService.Validate(
            new CompleteServiceCommand(Guid.NewGuid(), 12_000, null));

        Assert.Equal("La fecha del servicio es obligatoria.", Assert.Single(result.Errors).ErrorMessage);
    }

    [Fact]
    public void Complete_service_accepts_zero_kilometers_and_a_date()
    {
        ValidationResult result = _completeService.Validate(
            new CompleteServiceCommand(Guid.NewGuid(), 0, new DateOnly(2026, 7, 2)));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Odometer_reading_rejects_negative_kilometers()
    {
        ValidationResult result = _reading.Validate(
            new RegisterOdometerReadingCommand(Guid.NewGuid(), -1));

        Assert.Equal("Los kilómetros no pueden ser negativos.", Assert.Single(result.Errors).ErrorMessage);
    }

    [Fact]
    public void Odometer_reading_accepts_zero_kilometers()
    {
        ValidationResult result = _reading.Validate(
            new RegisterOdometerReadingCommand(Guid.NewGuid(), 0));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Quote_rejects_a_negative_amount()
    {
        ValidationResult result = _quote.Validate(new SubmitQuoteCommand(Guid.NewGuid(), -1m));

        Assert.Equal("El valor no puede ser negativo.", Assert.Single(result.Errors).ErrorMessage);
    }

    [Fact]
    public void Quote_accepts_zero()
    {
        ValidationResult result = _quote.Validate(new SubmitQuoteCommand(Guid.NewGuid(), 0m));

        Assert.True(result.IsValid);
    }
}
