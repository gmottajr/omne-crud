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
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalizedValue = value
            .Trim()
            .ToUpperInvariant();

        if (normalizedValue.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                $"SKU cannot exceed {MaxLength} characters.");
        }

        return new Sku(normalizedValue);
    }

    public override string ToString() => Value;
}
