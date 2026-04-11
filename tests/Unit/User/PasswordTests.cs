using Domain.User;

namespace Unit.User;

public class PasswordTests
{
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidPassword_ReturnsFailure(string password)
    {
        var result = Password.Create(password);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_PasswordShorterThanMinLength_ReturnsFailure()
    {
        var value = new string('a', Password.MinLength - 1);
        var result = Password.Create(value);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_PasswordExceedingMaxLength_ReturnsFailure()
    {
        var value = new string('a', Password.MaxLength + 1);
        var result = Password.Create(value);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_PasswordAtMinAndMaxLength_ReturnsSuccess()
    {
        var minPassword = new string('a', Password.MinLength);
        var maxPassword = new string('b', Password.MaxLength);

        var minResult = Password.Create(minPassword);
        var maxResult = Password.Create(maxPassword);

        Assert.True(minResult.IsSuccess);
        Assert.True(maxResult.IsSuccess);
        Assert.Equal(minPassword, minResult.Value.Value);
        Assert.Equal(maxPassword, maxResult.Value.Value);
    }
}