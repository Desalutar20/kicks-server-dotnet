using Domain.Shared.ValueObjects;

namespace Application.Abstractions.Payment;

public sealed record PaymentLineItem(
    NonEmptyString Name,
    Money Price,
    PositiveInt Quantity,
    NonEmptyString? Description,
    IReadOnlyList<Uri> Images
);
