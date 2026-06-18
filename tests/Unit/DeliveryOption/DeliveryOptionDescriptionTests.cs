using Domain.DeliveryOptions;
using FluentAssertions;

namespace Unit.DeliveryOption;

public class DeliveryOptionDescriptionTests
{
    [Theory]
    [InlineData("Standard delivery within 3-5 business days.")]
    [InlineData("Express delivery available within 24 hours.")]
    [InlineData("Pickup from our store during working hours.")]
    [InlineData("Courier delivery to your address within the city limits.")]
    [InlineData("   International shipping with tracking number included.   ")]
    public void Create_ValidDeliveryOptionDescription_ReturnsSuccess(
        string deliveryOptionDescription
    )
    {
        var result = DeliveryOptionDescription.Create(deliveryOptionDescription);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(deliveryOptionDescription.Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidDeliveryOptionDescription_ReturnsFailure(
        string deliveryOptionDescription
    )
    {
        var result = DeliveryOptionDescription.Create(deliveryOptionDescription);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_DeliveryOptionDescriptionExceedingMaxLength_ReturnsFailure()
    {
        var value = new string('a', DeliveryOptionDescription.MaxLength + 1);

        var result = DeliveryOptionDescription.Create(value);

        result.IsFailure.Should().BeTrue();
    }
}
