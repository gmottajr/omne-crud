using Omne_Crud_Demo.Core.Exceptions;

namespace Omne_Crud_Demo.Domain;

public sealed record Sku
{
    private const int MaxLength = 50;

    private Sku(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Sku Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidSkuException(
                "SKU cannot be null, empty, or whitespace.");
        }

        var normalizedValue = value
            .Trim()
            .ToUpperInvariant();

        if (normalizedValue.Length > MaxLength)
        {
            throw new InvalidSkuException(
                $"SKU cannot exceed {MaxLength} characters.");
        }

        return new Sku(normalizedValue);
    }

    public override string ToString() => Value;
}
