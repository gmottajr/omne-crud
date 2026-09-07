namespace Omne_Crud_Demo.Domain.Tests.ValueObjects;

public class SkuTests
{
    private const string SKU_CREATING = "ABC-123";

    [Fact]
    public void Create_ShouldCreateSku_WithValidValue()
    {
        var sku = Sku.Create(SKU_CREATING);

        Assert.Equal(SKU_CREATING, sku.Value);
    }

    [Fact]
    public void Create_ShouldTrimAndNormalizeValue()
    {
        var sku = Sku.Create("  abc-123  ");

        Assert.Equal(SKU_CREATING, sku.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_ShouldThrowArgumentException_WhenUsingWithEmptyValue(string value)
    {
        Assert.Throws<ArgumentException>(() => Sku.Create(value));
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenCreatingWithNullValue()
    {
        Assert.Throws<ArgumentNullException>(() => Sku.Create(null!));
    }

    [Fact]
    public void Create_ShouldThrowArgumentOutOfRangeException_WhenValueExceedingMaximumLength()
    {
        var value = new string('A', 51);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => Sku.Create(value));
    }

    [Fact]
    public void Create_ShouldBeEqual_WhenTwoSkus_WithSameNormalizedValue()
    {
        var first = Sku.Create("abc-123");
        var second = Sku.Create(" ABC-123 ");

        Assert.Equal(first, second);
    }

    [Fact]
    public void Create__ShouldNotBeEqualTwoSkus_WithDifferentValues()
    {
        var first = Sku.Create("ABC-123");
        var second = Sku.Create("XYZ-999");

        Assert.NotEqual(first, second);
    }
}
