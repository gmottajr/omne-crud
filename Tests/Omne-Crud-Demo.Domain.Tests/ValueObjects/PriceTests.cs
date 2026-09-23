using Omne_Crud_Demo.Core.Exceptions;

namespace Omne_Crud_Demo.Domain.Tests.ValueObjects;

public sealed class PriceTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(19.99)]
    public void Create_ShouldCreatePrice_WithValidValue(decimal value)
    {
        var price = Price.Create(value);

        Assert.Equal(value, price.Value);
    }

    [Fact]
    public void Create_ShouldThrowInvalidPriceException_WhenValueIsNegative()
    {
        Assert.Throws<InvalidPriceException>(() => Price.Create(-0.01m));
    }

    [Fact]
    public void Create_ShouldThrowInvalidPriceException_WhenValueHasMoreThanTwoDecimalPlaces()
    {
        Assert.Throws<InvalidPriceException>(() => Price.Create(19.999m));
    }
}