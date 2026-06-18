using Domain.Orders;
using Domain.Shared.ValueObjects;
using FluentAssertions;

namespace Unit.Order;

public class OrderPaymentTests
{
    private OrderPaymentTransactionId CreateValidTransactionId()
    {
        return OrderPaymentTransactionId.Create("TXN-20260614-0001").Value;
    }

    private Money CreateValidMoney()
    {
        return Money.FromDollars(100).Value;
    }

    [Fact]
    public void Create_ValidInput_ReturnsSuccess()
    {
        var transactionId = CreateValidTransactionId();
        var amount = CreateValidMoney();

        var result = OrderPayment.Create(transactionId, amount);

        result.IsSuccess.Should().BeTrue();
        result.Value.TransactionId.Should().Be(transactionId);
        result.Value.Amount.Should().Be(amount);
        result.Value.Status.Should().Be(OrderPaymentStatus.Pending);
    }

    [Fact]
    public void Create_SetsTimestamps_OnCreation()
    {
        var before = DateTimeOffset.UtcNow;

        var transactionId = CreateValidTransactionId();
        var amount = CreateValidMoney();

        var result = OrderPayment.Create(transactionId, amount);

        var after = DateTimeOffset.UtcNow;

        result.IsSuccess.Should().BeTrue();
        result.Value.CreatedAt.Should().BeOnOrAfter(before);
        result.Value.CreatedAt.Should().BeOnOrBefore(after);

        result.Value.UpdatedAt.Should().BeOnOrAfter(before);
        result.Value.UpdatedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Create_ZeroAmount_ReturnsFailure()
    {
        var transactionId = CreateValidTransactionId();
        var amount = Money.FromDollars(0).Value;

        var result = OrderPayment.Create(transactionId, amount);

        result.IsFailure.Should().BeTrue();
    }
}
