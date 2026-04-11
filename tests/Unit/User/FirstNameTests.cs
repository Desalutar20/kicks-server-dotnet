using Domain.User;

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

        Assert.True(result.IsSuccess);
        Assert.Equal(firstName.Trim(), result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidFirstName_ReturnsFailure(string firstName)
    {
        var result = FirstName.Create(firstName);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_FirstNameExceedingMaxLength_ReturnsFailure()
    {
        var value = new string('a', FirstName.MaxLength + 1);

        var result = FirstName.Create(value);

        Assert.True(result.IsFailure);
    }
}