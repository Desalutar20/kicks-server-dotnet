using Domain.Users;
using FluentAssertions;

namespace Unit.User;

public class FirstNameTests
{
    [Theory]
    [InlineData("Alice")]
    [InlineData("Bob")]
    [InlineData("Jean Paul")]
    [InlineData("Élise")]
    [InlineData("   Alice   ")]
    public void Create_ValidFirstName_ReturnsSuccess(string firstName)
    {
        var result = FirstName.Create(firstName);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(firstName.Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidFirstName_ReturnsFailure(string firstName)
    {
        var result = FirstName.Create(firstName);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_FirstNameExceedingMaxLength_ReturnsFailure()
    {
        var value = new string('a', FirstName.MaxLength + 1);

        var result = FirstName.Create(value);

        result.IsFailure.Should().BeTrue();
    }
}
