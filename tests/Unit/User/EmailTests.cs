using Domain.User;

namespace Unit.User;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name+tag+sorting@example.co.uk")]
    [InlineData("email@subdomain.example.com")]
    public void Create_ValidEmail_ReturnsSuccess(string email)
    {
        var result = Email.Create(email);

        Assert.True(result.IsSuccess);
        Assert.Equal(email, result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("plainaddress")]
    [InlineData("@missingusername.com")]
    [InlineData("email@example,com")]
    [InlineData("email@.example.com")]
    public void Create_InvalidEmail_ReturnsFailure(string email)
    {
        var result = Email.Create(email);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_EmailExceedingMaxLength_ReturnsFailure()
    {
        var localPart = new string('a', Email.MaxLength + 1);
        var email = localPart + "@example.com";

        var result = Email.Create(email);

        Assert.True(result.IsFailure);
    }
}