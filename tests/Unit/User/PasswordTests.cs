using Domain.Users;
using FluentAssertions;

namespace Unit.User;

public class PasswordTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidPassword_ReturnsFailure(string password)
    {
        var result = Password.Create(password);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_PasswordShorterThanMinLength_ReturnsFailure()
    {
        var value = new string('a', Password.MinLength - 1);

        var result = Password.Create(value);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_PasswordExceedingMaxLength_ReturnsFailure()
    {
        var value = new string('a', Password.MaxLength + 1);

        var result = Password.Create(value);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_PasswordAtMinAndMaxLength_ReturnsSuccess()
    {
        var minPassword = new string('a', Password.MinLength);
        var maxPassword = new string('b', Password.MaxLength);

        var minResult = Password.Create(minPassword);
        var maxResult = Password.Create(maxPassword);

        minResult.IsSuccess.Should().BeTrue();
        maxResult.IsSuccess.Should().BeTrue();

        minResult.Value.Value.Should().Be(minPassword);
        maxResult.Value.Value.Should().Be(maxPassword);
    }
}
