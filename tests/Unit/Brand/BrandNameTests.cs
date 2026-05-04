using Domain.Product.Brand;

namespace Unit.Brand;

public class BrandNameTests
{
    [Theory]
    [InlineData("Nike")]
    [InlineData("Apple")]
    [InlineData("Samsung")]
    [InlineData("H&M")]
    [InlineData("   Nike   ")]
    public void Create_ValidBrandName_ReturnsSuccess(string brandName)
    {
        var result = BrandName.Create(brandName);

        Assert.True(result.IsSuccess);
        Assert.Equal(brandName.Trim(), result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidBrandName_ReturnsFailure(string brandName)
    {
        var result = BrandName.Create(brandName);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_BrandNameExceedingMaxLength_ReturnsFailure()
    {
        var value = new string('a', BrandName.MaxLength + 1);

        var result = BrandName.Create(value);

        Assert.True(result.IsFailure);
    }
}
