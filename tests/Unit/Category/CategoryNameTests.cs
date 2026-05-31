using Domain.Categories;
using FluentAssertions;

namespace Unit.Category;

public class CategoryNameTests
{
    [Theory]
    [InlineData("Electronics")]
    [InlineData("Home Appliances")]
    [InlineData("Books")]
    [InlineData("Health & Beauty")]
    [InlineData("   Electronics   ")]
    public void Create_ValidCategoryName_ReturnsSuccess(string categoryName)
    {
        var result = CategoryName.Create(categoryName);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(categoryName.Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidCategoryName_ReturnsFailure(string categoryName)
    {
        var result = CategoryName.Create(categoryName);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_CategoryNameExceedingMaxLength_ReturnsFailure()
    {
        var value = new string('a', CategoryName.MaxLength + 1);

        var result = CategoryName.Create(value);

        result.IsFailure.Should().BeTrue();
    }
}
