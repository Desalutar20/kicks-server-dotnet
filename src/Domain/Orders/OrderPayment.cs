using Domain.Abstractions;
using Domain.Shared.ValueObjects;

namespace Domain.Orders;

public sealed class OrderPayment
{
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public OrderPaymentTransactionId TransactionId { get; private set; }
    public Money Amount { get; private set; }
    public OrderPaymentStatus Status { get; private set; } = OrderPaymentStatus.Pending;

    private OrderPayment(OrderPaymentTransactionId transactionId, Money amount)
    {
        TransactionId = transactionId;
        Amount = amount;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static Result<OrderPayment> Create(OrderPaymentTransactionId transactionId, Money amount)
    {
        if (amount.Cents <= 0)
        {
            return Error.Failure("Payment amount must be greater than zero.");
        }

        return new OrderPayment(transactionId, amount);
    }
}
