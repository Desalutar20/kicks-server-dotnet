using Domain.Brands;
using FluentAssertions;

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

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(brandName.Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidBrandName_ReturnsFailure(string brandName)
    {
        var result = BrandName.Create(brandName);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_BrandNameExceedingMaxLength_ReturnsFailure()
    {
        var value = new string('a', BrandName.MaxLength + 1);

        var result = BrandName.Create(value);

        result.IsFailure.Should().BeTrue();
    }
}
