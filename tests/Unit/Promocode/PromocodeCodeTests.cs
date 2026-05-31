using System.Text.RegularExpressions;
using Domain.Promocodes;
using FluentAssertions;

namespace Unit.Promocode;

public class PromocodeCodeTests
{
    private static readonly Regex Regex = new(@"\s+");

    [Theory]
    [InlineData("SUMMER25")]
    [InlineData("   SUMMER 25   ")]
    public void Create_ValidPromocodeCode_ReturnsSuccess(string code)
    {
        var result = PromocodeCode.Create(code);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(Regex.Replace(code, ""));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidPromocodeCode_ReturnsFailure(string code)
    {
        var result = PromocodeCode.Create(code);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_PromocodeCodeExceedingMaxLength_ReturnsFailure()
    {
        var value = new string('a', PromocodeCode.MaxLength + 1);

        var result = PromocodeCode.Create(value);

        result.IsFailure.Should().BeTrue();
    }
}
