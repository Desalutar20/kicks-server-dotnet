using Domain.Product;

namespace Unit.Product;

public class ProductTitleTests
{
    [Theory]
    [InlineData("Nike Air Max 90")]
    [InlineData("Adidas Ultraboost 22")]
    [InlineData("Puma Suede Classic")]
    [InlineData("New Balance 550")]
    [InlineData("   Converse Chuck Taylor All Star   ")]
    public void Create_ValidProductTitle_ReturnsSuccess(string productTitle)
    {
        var result = ProductTitle.Create(productTitle);

        Assert.True(result.IsSuccess);
        Assert.Equal(productTitle.Trim(), result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidProductTitle_ReturnsFailure(string productTitle)
    {
        var result = ProductTitle.Create(productTitle);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_ProductTitleExceedingMaxLength_ReturnsFailure()
    {
        var value = new string('a', ProductTitle.MaxLength + 1);

        var result = ProductTitle.Create(value);
        Assert.True(result.IsFailure);
    }
}
