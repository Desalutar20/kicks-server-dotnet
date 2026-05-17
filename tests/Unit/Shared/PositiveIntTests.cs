using Domain.Shared;

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

        Assert.True(result.IsSuccess);

        Assert.Equal(value, result.Value.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void Create_InvalidPositiveInt_ReturnsFailure(int value)
    {
        var result = PositiveInt.Create(value);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Addition_ReturnsCorrectResult()
    {
        var left = PositiveInt.Create(10).Value;
        var right = PositiveInt.Create(5).Value;

        var result = left + right;

        Assert.Equal(15, result.Value);
    }

    [Fact]
    public void Subtraction_ReturnsCorrectResult()
    {
        var left = PositiveInt.Create(10).Value;
        var right = PositiveInt.Create(5).Value;

        var result = left - right;

        Assert.Equal(5, result.Value);
    }

    [Fact]
    public void Multiplication_ReturnsCorrectResult()
    {
        var left = PositiveInt.Create(10).Value;
        var right = PositiveInt.Create(5).Value;

        var result = left * right;

        Assert.Equal(50, result.Value);
    }

    [Fact]
    public void Division_ReturnsCorrectResult()
    {
        var left = PositiveInt.Create(10).Value;
        var right = PositiveInt.Create(5).Value;

        var result = left / right;

        Assert.Equal(2, result.Value);
    }

    [Fact]
    public void GreaterThan_ReturnsTrue_WhenLeftIsGreater()
    {
        var left = PositiveInt.Create(10).Value;
        var right = PositiveInt.Create(5).Value;

        var result = left > right;

        Assert.True(result);
    }

    [Fact]
    public void LessThan_ReturnsTrue_WhenLeftIsLess()
    {
        var left = PositiveInt.Create(5).Value;
        var right = PositiveInt.Create(10).Value;

        var result = left < right;

        Assert.True(result);
    }

    [Fact]
    public void GreaterThanOrEqual_ReturnsTrue_WhenValuesAreEqual()
    {
        var left = PositiveInt.Create(10).Value;
        var right = PositiveInt.Create(10).Value;

        var result = left >= right;

        Assert.True(result);
    }

    [Fact]
    public void LessThanOrEqual_ReturnsTrue_WhenValuesAreEqual()
    {
        var left = PositiveInt.Create(10).Value;
        var right = PositiveInt.Create(10).Value;

        var result = left <= right;

        Assert.True(result);
    }
}
