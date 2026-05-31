using Domain.Users;
using FluentAssertions;

namespace Unit.User;

public class HashedPasswordTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidHashedPassword_ReturnsFailure(string hashedPassword)
    {
        var result = HashedPassword.Create(hashedPassword);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_HashedPasswordShorterThanMinLength_ReturnsFailure()
    {
        var value = new string('a', HashedPassword.MinLength - 1);

        var result = HashedPassword.Create(value);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_HashedPasswordExceedingMaxLength_ReturnsFailure()
    {
        var value = new string('b', HashedPassword.MaxLength + 1);

        var result = HashedPassword.Create(value);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_HashedPasswordAtMinAndMaxLength_ReturnsSuccess()
    {
        var minPassword = new string('b', HashedPassword.MinLength);
        var maxPassword = new string('b', HashedPassword.MaxLength);

        var minResult = HashedPassword.Create(minPassword);
        var maxResult = HashedPassword.Create(maxPassword);

        minResult.IsSuccess.Should().BeTrue();
        maxResult.IsSuccess.Should().BeTrue();

        minResult.Value.Value.Should().Be(minPassword);
        maxResult.Value.Value.Should().Be(maxPassword);
    }

    [Fact]
    public void Create_HashedPasswordWithSpaces_TrimsSpaces()
    {
        var value = "   " + new string('c', HashedPassword.MinLength) + "   ";

        var result = HashedPassword.Create(value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value.Trim());
    }
}
