using Domain.Product;

namespace Unit.Product;

public class ProductDescriptionTests
{
    [Theory]
    [InlineData("Classic running sneakers with lightweight cushioning and durable rubber outsole.")]
    [InlineData("Premium leather basketball shoes designed for everyday streetwear.")]
    [InlineData("Breathable mesh upper with responsive foam midsole for maximum comfort.")]
    [InlineData("Minimalist casual sneakers suitable for daily use and long walks.")]
    [InlineData("   High-performance training shoes with excellent grip and ankle support.   ")]
    public void Create_ValidProductDescription_ReturnsSuccess(string productDescription)
    {
        var result = ProductDescription.Create(productDescription);

        Assert.True(result.IsSuccess);
        Assert.Equal(productDescription.Trim(), result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidProductDescription_ReturnsFailure(string productDescription)
    {
        var result = ProductDescription.Create(productDescription);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_ProductDescriptionExceedingMaxLength_ReturnsFailure()
    {
        var value = new string('a', ProductDescription.MaxLength + 1);

        var result = ProductDescription.Create(value);
        Assert.True(result.IsFailure);
    }
}
