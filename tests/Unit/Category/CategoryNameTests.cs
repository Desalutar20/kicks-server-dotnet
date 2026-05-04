using Domain.Product.Category;

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

        Assert.True(result.IsSuccess);
        Assert.Equal(categoryName.Trim(), result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidCategoryName_ReturnsFailure(string categoryName)
    {
        var result = CategoryName.Create(categoryName);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_CategoryNameExceedingMaxLength_ReturnsFailure()
    {
        var value = new string('a', CategoryName.MaxLength + 1);

        var result = CategoryName.Create(value);

        Assert.True(result.IsFailure);
    }
}
