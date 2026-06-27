using Domain.Shared.ValueObjects;

namespace Application.Abstractions.Payment;

public sealed record CreatePaymentResponse(NonEmptyString PaymentId, Uri CheckoutUrl);
