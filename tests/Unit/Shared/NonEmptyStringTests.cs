using Domain.Shared;
using FluentAssertions;

namespace Unit.Shared;

public class NonEmptyStringTests
{
    [Theory]
    [InlineData("Hello")]
    [InlineData("Test")]
    [InlineData("123")]
    public void Create_ValidString_ReturnsSuccess(string value)
    {
        var result = NonEmptyString.Create(value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("  Hello  ", "Hello")]
    [InlineData("\tTest\t", "Test")]
    [InlineData("\nValue\n", "Value")]
    public void Create_StringWithWhitespace_TrimsValue(string value, string expected)
    {
        var result = NonEmptyString.Create(value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_EmptyString_ReturnsFailure(string value)
    {
        var result = NonEmptyString.Create(value);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ToString_ReturnsUnderlyingValue()
    {
        var nonEmptyString = NonEmptyString.Create("Hello").Value;

        var result = nonEmptyString.ToString();

        result.Should().Be("Hello");
    }
}
