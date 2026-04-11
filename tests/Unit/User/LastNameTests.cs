using Domain.User;

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

        Assert.True(result.IsSuccess);
        Assert.Equal(lastName.Trim(), result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidLastName_ReturnsFailure(string lastName)
    {
        var result = LastName.Create(lastName);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_LastNameExceedingMaxLength_ReturnsFailure()
    {
        var value = new string('a', LastName.MaxLength + 1);

        var result = LastName.Create(value);

        Assert.True(result.IsFailure);
    }
}