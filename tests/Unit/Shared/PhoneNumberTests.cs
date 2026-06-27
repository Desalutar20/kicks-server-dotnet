using Domain.Shared.ValueObjects;
using FluentAssertions;

namespace Unit.Shared;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("+12025550123")]
    [InlineData("+15551234567")]
    [InlineData("+19999999999")]
    public void Create_ValidPhoneNumber_ReturnsSuccess(string phoneNumber)
    {
        var result = PhoneNumber.Create(phoneNumber);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(phoneNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12025550123")]
    [InlineData("+2025550123")]
    [InlineData("+1202555012")]
    [InlineData("+120255501234")]
    [InlineData("+1-202-555-0123")]
    [InlineData("(202)5550123")]
    [InlineData("+442071838750")]
    [InlineData("abc")]
    public void Create_InvalidPhoneNumber_ReturnsFailure(string phoneNumber)
    {
        var result = PhoneNumber.Create(phoneNumber);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_PhoneNumberExceedingMaxLength_ReturnsFailure()
    {
        var phoneNumber = "+" + new string('1', PhoneNumber.MaxLength + 1);

        var result = PhoneNumber.Create(phoneNumber);

        result.IsFailure.Should().BeTrue();
    }
}
