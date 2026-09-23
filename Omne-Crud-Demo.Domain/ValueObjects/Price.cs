using Omne_Crud_Demo.Core.Exceptions;

namespace Omne_Crud_Demo.Domain;

public sealed record Price
{
    private const int DecimalPlaces = 2;

    private Price(decimal value)
    {
        Value = decimal.Round(value, DecimalPlaces);
    }

    public decimal Value { get; }

    public static Price Create(decimal value)
    {
        if (value < 0)
        {
            throw new InvalidPriceException(
                "Price cannot be less than zero.");
        }

        if (value != decimal.Round(value, DecimalPlaces))
        {
            throw new InvalidPriceException(
                "Price cannot have more than two decimal places.");
        }

        return new Price(value);
    }

    public override string ToString() => Value.ToString("F2");
}
