namespace Unit.Shared;

using Domain.Shared.ValueObjects;
using FluentAssertions;
using Xunit;

public class MoneyTests
{
    [Fact]
    public void FromCents_ShouldCreateMoney_WhenPositive()
    {
        var result = Money.FromCents(150);

        result.IsSuccess.Should().BeTrue();
        result.Value.Cents.Should().Be(150);
    }

    [Fact]
    public void FromCents_ShouldFail_WhenNegative()
    {
        var result = Money.FromCents(-1);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void FromDollars_ShouldConvertCorrectly()
    {
        var result = Money.FromDollars(10.25m);

        result.IsSuccess.Should().BeTrue();
        result.Value.Cents.Should().Be(1025);
    }

    [Fact]
    public void Dollars_ShouldReturnCorrectValue()
    {
        var money = Money.FromCents(1050).Value;

        money.Dollars.Should().Be(10.50m);
    }

    [Fact]
    public void Addition_ShouldSumCents()
    {
        var a = Money.FromCents(100).Value;
        var b = Money.FromCents(250).Value;

        var result = a + b;

        result.Cents.Should().Be(350);
    }

    [Fact]
    public void Subtraction_ShouldWork_WhenPositiveResult()
    {
        var a = Money.FromCents(500).Value;
        var b = Money.FromCents(200).Value;

        var result = a - b;

        result.Cents.Should().Be(300);
    }

    [Fact]
    public void Subtraction_ShouldThrow_WhenNegative()
    {
        var a = Money.FromCents(100).Value;
        var b = Money.FromCents(200).Value;

        Action act = () => _ = a - b;

        act.Should().Throw<InvalidOperationException>().WithMessage("Money cannot be negative.");
    }

    [Fact]
    public void Comparison_ShouldWork()
    {
        var a = Money.FromCents(200).Value;
        var b = Money.FromCents(100).Value;

        (a > b).Should().BeTrue();
        (a < b).Should().BeFalse();
        (a >= b).Should().BeTrue();
        (a <= b).Should().BeFalse();
    }

    [Fact]
    public void FromDollars_ShouldRoundCorrectly()
    {
        var money = Money.FromDollars(10.999m).Value;

        money.Cents.Should().Be(1100);
    }
}
