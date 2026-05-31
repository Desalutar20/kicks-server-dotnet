using Domain.Users;
using FluentAssertions;

namespace Unit.User;

public class LastNameTests
{
    [Theory]
    [InlineData("Alice")]
    [InlineData("Bob")]
    [InlineData("Jean Paul")]
    [InlineData("Élise")]
    [InlineData("   Alice   ")]
    public void Create_ValidLastName_ReturnsSuccess(string lastName)
    {
        var result = LastName.Create(lastName);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(lastName.Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidLastName_ReturnsFailure(string lastName)
    {
        var result = LastName.Create(lastName);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_LastNameExceedingMaxLength_ReturnsFailure()
    {
        var value = new string('a', LastName.MaxLength + 1);

        var result = LastName.Create(value);

        result.IsFailure.Should().BeTrue();
    }
}
