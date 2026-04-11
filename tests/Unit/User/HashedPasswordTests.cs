using Domain.User;

namespace Unit.User;

public class HashedPasswordTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidHashedPassword_ReturnsFailure(string hashedPassword)
    {
        var result = HashedPassword.Create(hashedPassword);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_HashedPasswordShorterThanMinLength_ReturnsFailure()
    {
        var value = new string('a', HashedPassword.MinLength - 1);
        var result = HashedPassword.Create(value);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_HashedPasswordExceedingMaxLength_ReturnsFailure()
    {
        var value = new string('b', HashedPassword.MaxLength + 1);
        var result = HashedPassword.Create(value);

        Assert.True(result.IsFailure);
    }


    [Fact]
    public void Create_HashedPasswordAtMinAndMaxLength_ReturnsSuccess()
    {
        var minPassword = new string('b', HashedPassword.MinLength);
        var maxPassword = new string('b', HashedPassword.MaxLength);

        var minResult = HashedPassword.Create(minPassword);
        var maxResult = HashedPassword.Create(maxPassword);

        Assert.True(minResult.IsSuccess);
        Assert.True(maxResult.IsSuccess);
        Assert.Equal(minPassword, minResult.Value.Value);
        Assert.Equal(maxPassword, maxResult.Value.Value);
    }

    [Fact]
    public void Create_HashedPasswordWithSpaces_TrimsSpaces()
    {
        var value = "   " + new string('c', HashedPassword.MinLength) + "   ";
        var result = HashedPassword.Create(value);

        Assert.True(result.IsSuccess);
        Assert.Equal(value.Trim(), result.Value.Value);
    }
}