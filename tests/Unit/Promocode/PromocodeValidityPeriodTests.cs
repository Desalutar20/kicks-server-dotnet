using Domain.Promocodes;
using FluentAssertions;

namespace Unit.Promocode;

public sealed class PromocodeValidityPeriodTests
{
    [Fact]
    public void Create_Should_ReturnSuccess_WhenValidToIsGreaterThanValidFrom()
    {
        var validFrom = DateTimeOffset.UtcNow;
        var validTo = validFrom.AddDays(1);

        var result = PromocodeValidityPeriod.Create(validFrom, validTo);

        result.IsSuccess.Should().BeTrue();

        result.Value.ValidFrom.Should().Be(validFrom);
        result.Value.ValidTo.Should().Be(validTo);
    }

    [Fact]
    public void Create_Should_ReturnFailure_WhenValidToEqualsValidFrom()
    {
        var validFrom = DateTimeOffset.UtcNow;
        var validTo = validFrom;

        var result = PromocodeValidityPeriod.Create(validFrom, validTo);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_Should_ReturnFailure_WhenValidToIsLessThanValidFrom()
    {
        var validFrom = DateTimeOffset.UtcNow;
        var validTo = validFrom.AddDays(-1);

        var result = PromocodeValidityPeriod.Create(validFrom, validTo);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_Should_ReturnFailure_WhenValidTLessThanNow()
    {
        var validFrom = DateTimeOffset.UtcNow.AddDays(-2);
        var validTo = validFrom.AddDays(1);

        var result = PromocodeValidityPeriod.Create(validFrom, validTo);

        result.IsFailure.Should().BeTrue();
    }
}
