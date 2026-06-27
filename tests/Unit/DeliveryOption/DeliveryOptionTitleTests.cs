using Domain.DeliveryOptions;
using FluentAssertions;

namespace Unit.DeliveryOption;

public class DeliveryOptionTitleTests
{
    [Theory]
    [InlineData("Standard Delivery")]
    [InlineData("Express Delivery")]
    [InlineData("Store Pickup")]
    [InlineData("Courier Delivery")]
    [InlineData("International Shipping")]
    [InlineData("   Same Day Delivery   ")]
    public void Create_ValidDeliveryOptionTitle_ReturnsSuccess(string deliveryOptionTitle)
    {
        var result = DeliveryOptionTitle.Create(deliveryOptionTitle);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(deliveryOptionTitle.Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidDeliveryOptionTitle_ReturnsFailure(string deliveryOptionTitle)
    {
        var result = DeliveryOptionTitle.Create(deliveryOptionTitle);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_DeliveryOptionTitleExceedingMaxLength_ReturnsFailure()
    {
        var value = new string('a', DeliveryOptionTitle.MaxLength + 1);

        var result = DeliveryOptionTitle.Create(value);
        result.IsFailure.Should().BeTrue();
    }
}
