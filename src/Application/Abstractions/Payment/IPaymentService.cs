using Domain.Orders;

namespace Application.Abstractions.Payment;

public interface IPaymentService
{
    Task<CreatePaymentResponse> CreatePaymentAsync(
        CreatePaymentRequest request,
        CancellationToken ct = default
    );

    // Task<PaymentStatusResponse?> GetPaymentAsync(
    //     NonEmptyString paymentId,
    //     CancellationToken ct = default
    // );
}
