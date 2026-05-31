using Domain.Promocodes;
using Domain.Shared;
using FluentAssertions;

namespace Unit.Promocode;

public sealed class PromocodeTests
{
    [Fact]
    public void Create_Should_ReturnSuccess_WhenPercentDiscountIsBetween1And99()
    {
        var validityPeriod = PromocodeValidityPeriod
            .Create(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1))
            .Value;

        var result = Domain.Promocodes.Promocode.Create(
            PositiveInt.Create(50).Value,
            PromocodeType.Percent,
            validityPeriod,
            PositiveInt.Create(100).Value,
            PromocodeCode.Create("SUMMER50").Value
        );

        result.IsSuccess.Should().BeTrue();

        result.Value.DiscountValue.Value.Should().Be(50);
        result.Value.Type.Should().Be(PromocodeType.Percent);
        result.Value.UsageCount.Should().Be(0);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(101)]
    public void Create_Should_ReturnFailure_WhenPercentDiscountIsOutOfRange(int discountValue)
    {
        var validityPeriod = PromocodeValidityPeriod
            .Create(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1))
            .Value;

        var result = Domain.Promocodes.Promocode.Create(
            PositiveInt.Create(discountValue).Value,
            PromocodeType.Percent,
            validityPeriod,
            PositiveInt.Create(100).Value,
            PromocodeCode.Create("SUMMER50").Value
        );

        result.IsFailure.Should().BeTrue();
    }
}
