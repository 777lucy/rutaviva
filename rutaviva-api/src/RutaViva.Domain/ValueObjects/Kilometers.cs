using RutaViva.Domain.Exceptions;

namespace RutaViva.Domain.ValueObjects;

public readonly record struct Kilometers
{
    public Kilometers(int value)
    {
        if (value < 0)
        {
            throw new DomainException("kilometers.negative", "Los kilómetros no pueden ser negativos.");
        }

        Value = value;
    }

    public int Value { get; }

    public bool IsGreaterThan(Kilometers other) => Value > other.Value;
}
