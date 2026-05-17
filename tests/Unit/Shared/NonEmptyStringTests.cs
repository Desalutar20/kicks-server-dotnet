using Domain.Shared;

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

        Assert.True(result.IsSuccess);

        Assert.Equal(value, result.Value.Value);
    }

    [Theory]
    [InlineData("  Hello  ", "Hello")]
    [InlineData("\tTest\t", "Test")]
    [InlineData("\nValue\n", "Value")]
    public void Create_StringWithWhitespace_TrimmsValue(string value, string expected)
    {
        var result = NonEmptyString.Create(value);

        Assert.True(result.IsSuccess);

        Assert.Equal(expected, result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_EmptyString_ReturnsFailure(string value)
    {
        var result = NonEmptyString.Create(value);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ToString_ReturnsUnderlyingValue()
    {
        var nonEmptyString = NonEmptyString.Create("Hello").Value;

        var result = nonEmptyString.ToString();

        Assert.Equal("Hello", result);
    }
}
