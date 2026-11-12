using RutaViva.Domain.Exceptions;

namespace RutaViva.Domain.ValueObjects;

public readonly record struct Money
{
    public const decimal ManualApprovalThreshold = 1_500_000m;

    public Money(decimal amount)
    {
        if (amount < 0)
        {
            throw new DomainException("money.negative", "El valor no puede ser negativo.");
        }

        Amount = amount;
    }

    public decimal Amount { get; }

    public bool IsGreaterThan(Money other) => Amount > other.Amount;
}
