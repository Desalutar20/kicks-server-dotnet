using Domain.Orders;
using FluentAssertions;

namespace Unit.Order;

public class OrderPaymentTransactionIdTests
{
    [Theory]
    [InlineData("TXN-20260614-0001")]
    [InlineData("order_9F3A1B2C")]
    [InlineData("PAY-INV-10004567")]
    [InlineData("stripe_pi_3NQ9xY2eZvKYlo2C0")]
    [InlineData("paypal-transaction-8KJH4G7L")]
    [InlineData("txn_000000000123456789")]
    [InlineData("ORDER-AR-2026-0001")]
    [InlineData("pgw:txn:8f3c9a1d")]
    public void Create_ValidProductTitle_ReturnsSuccess(string productTitle)
    {
        var result = OrderPaymentTransactionId.Create(productTitle);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(productTitle.Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidProductTitle_ReturnsFailure(string id)
    {
        var result = OrderPaymentTransactionId.Create(id);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_ProductTitleExceedingMaxLength_ReturnsFailure()
    {
        var value = new string('a', OrderPaymentTransactionId.MaxLength + 1);

        var result = OrderPaymentTransactionId.Create(value);
        result.IsFailure.Should().BeTrue();
    }
}
