using Domain.Products;
using FluentAssertions;

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

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(productTitle.Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidProductTitle_ReturnsFailure(string productTitle)
    {
        var result = ProductTitle.Create(productTitle);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_ProductTitleExceedingMaxLength_ReturnsFailure()
    {
        var value = new string('a', ProductTitle.MaxLength + 1);

        var result = ProductTitle.Create(value);
        result.IsFailure.Should().BeTrue();
    }
}
