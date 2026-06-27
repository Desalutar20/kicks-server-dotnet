using Domain.Shared.ValueObjects;

namespace Application.Abstractions.Payment;

public sealed record CreatePaymentRequest(
    Domain.Shared.ValueObjects.Email Email,
    NonEmptyString Currency,
    Money AmountCents,
    IReadOnlyList<PaymentLineItem> Items,
    Uri SuccessUrl,
    Uri CancelUrl,
    Dictionary<string, string>? Metadata
);
