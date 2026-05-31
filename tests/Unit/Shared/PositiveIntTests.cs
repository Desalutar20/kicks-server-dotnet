using Domain.Shared;
using FluentAssertions;

namespace Unit.Shared;

public class PositiveIntTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(int.MaxValue)]
    public void Create_ValidPositiveInt_ReturnsSuccess(int value)
    {
        var result = PositiveInt.Create(value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void Create_InvalidPositiveInt_ReturnsFailure(int value)
    {
        var result = PositiveInt.Create(value);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Addition_ReturnsCorrectResult()
    {
        var left = PositiveInt.Create(10).Value;
        var right = PositiveInt.Create(5).Value;

        var result = left + right;

        result.Value.Should().Be(15);
    }

    [Fact]
    public void Subtraction_ReturnsCorrectResult()
    {
        var left = PositiveInt.Create(10).Value;
        var right = PositiveInt.Create(5).Value;

        var result = left - right;

        result.Value.Should().Be(5);
    }

    [Fact]
    public void Multiplication_ReturnsCorrectResult()
    {
        var left = PositiveInt.Create(10).Value;
        var right = PositiveInt.Create(5).Value;

        var result = left * right;

        result.Value.Should().Be(50);
    }

    [Fact]
    public void Division_ReturnsCorrectResult()
    {
        var left = PositiveInt.Create(10).Value;
        var right = PositiveInt.Create(5).Value;

        var result = left / right;

        result.Value.Should().Be(2);
    }

    [Fact]
    public void GreaterThan_ReturnsTrue_WhenLeftIsGreater()
    {
        var left = PositiveInt.Create(10).Value;
        var right = PositiveInt.Create(5).Value;

        (left > right).Should().BeTrue();
    }

    [Fact]
    public void LessThan_ReturnsTrue_WhenLeftIsLess()
    {
        var left = PositiveInt.Create(5).Value;
        var right = PositiveInt.Create(10).Value;

        (left < right).Should().BeTrue();
    }

    [Fact]
    public void GreaterThanOrEqual_ReturnsTrue_WhenValuesAreEqual()
    {
        var left = PositiveInt.Create(10).Value;
        var right = PositiveInt.Create(10).Value;

        (left >= right).Should().BeTrue();
    }

    [Fact]
    public void LessThanOrEqual_ReturnsTrue_WhenValuesAreEqual()
    {
        var left = PositiveInt.Create(10).Value;
        var right = PositiveInt.Create(10).Value;

        (left <= right).Should().BeTrue();
    }
}
