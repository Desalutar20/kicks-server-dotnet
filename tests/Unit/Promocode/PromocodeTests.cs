using Domain.Promocodes;
using Domain.Shared;
using Domain.Shared.ValueObjects;
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

    [Fact]
    public void CalculateDiscount_Should_ReturnPercentDiscount()
    {
        var promocode = CreatePromocode(discountValue: 20, type: PromocodeType.Percent);

        var subtotal = Money.FromCents(10_000).Value;

        var discount = promocode.CalculateDiscount(subtotal);

        discount.Cents.Should().Be(2_000);
    }

    [Fact]
    public void CalculateDiscount_Should_ReturnFixedDiscount()
    {
        var promocode = CreatePromocode(discountValue: 25, type: PromocodeType.Fixed);

        var subtotal = Money.FromCents(10_000).Value;

        var discount = promocode.CalculateDiscount(subtotal);

        discount.Cents.Should().Be(2_500);
    }

    [Fact]
    public void CalculateDiscount_Should_NotExceedSubtotal()
    {
        var promocode = CreatePromocode(discountValue: 100, type: PromocodeType.Fixed);

        var subtotal = Money.FromCents(5_000).Value;

        var discount = promocode.CalculateDiscount(subtotal);

        discount.Cents.Should().Be(5_000);
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

    private static Domain.Promocodes.Promocode CreatePromocode(
        int discountValue = 20,
        PromocodeType type = PromocodeType.Percent
    )
    {
        var validityPeriod = PromocodeValidityPeriod
            .Create(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(10))
            .Value;

        return Domain
            .Promocodes.Promocode.Create(
                PositiveInt.Create(discountValue).Value,
                type,
                validityPeriod,
                PositiveInt.Create(100).Value,
                PromocodeCode.Create("SUMMER20").Value
            )
            .Value;
    }
}
