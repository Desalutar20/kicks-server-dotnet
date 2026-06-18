using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Orders;

public sealed record OrderPaymentTransactionId
{
    public const int MaxLength = 60;

    public string Value { get; }

    private OrderPaymentTransactionId(string value)
    {
        Value = value;
    }

    public static Result<OrderPaymentTransactionId> Create(string value)
    {
        var emptyResult = Guard.AgainstEmptyString(value);
        if (emptyResult.IsFailure)
        {
            return Error.Internal(emptyResult.Error.Description);
        }

        value = value.Trim();

        var lengthResult = Guard.ForStringLength(value, 1, MaxLength, "Payment transaction id");
        if (lengthResult.IsFailure)
        {
            return Error.Internal(lengthResult.Error.Description);
        }

        return new OrderPaymentTransactionId(value);
    }

    public override string ToString() => Value;
};
