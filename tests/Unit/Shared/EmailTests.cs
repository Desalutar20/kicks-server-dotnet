using Domain.Shared.ValueObjects;
using Domain.Users;
using FluentAssertions;

namespace Unit.Shared;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name+tag+sorting@example.co.uk")]
    [InlineData("email@subdomain.example.com")]
    public void Create_ValidEmail_ReturnsSuccess(string email)
    {
        var result = Email.Create(email);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(email);
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

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_EmailExceedingMaxLength_ReturnsFailure()
    {
        var localPart = new string('a', Email.MaxLength + 1);
        var email = localPart + "@example.com";

        var result = Email.Create(email);

        result.IsFailure.Should().BeTrue();
    }
}
