using Application.Abstractions.Payment;
using Domain.Shared.ValueObjects;
using Stripe;
using Stripe.Checkout;

namespace Infrastructure.Services;

internal sealed class PaymentService(Config config) : IPaymentService
{
    private readonly SessionService _sessionService = new(new StripeClient(config.Stripe.Secret));

    public async Task<CreatePaymentResponse> CreatePaymentAsync(
        CreatePaymentRequest request,
        CancellationToken ct = default
    )
    {
        var lineItems = request
            .Items.Select(x =>
            {
                var productData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = x.Name.Value,
                    Description = x.Description?.Value,
                };

                if (x.Images.Count > 0)
                {
                    productData.Images = x.Images.Select(image => image.ToString()).ToList();
                }

                return new SessionLineItemOptions
                {
                    Quantity = x.Quantity.Value,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = request.Currency.Value,
                        UnitAmount = x.Price.Cents,
                        ProductData = productData,
                    },
                };
            })
            .ToList();

        var metadata = request.Metadata ?? [];

        var options = new SessionCreateOptions
        {
            Mode = "payment",
            Currency = request.Currency.Value,
            CustomerEmail = request.Email.Value,
            SuccessUrl = request.SuccessUrl.ToString(),
            CancelUrl = request.CancelUrl.ToString(),

            ClientReferenceId = metadata.GetValueOrDefault("order_id"),
            Metadata = metadata,
            LineItems = lineItems,
        };

        var session = await _sessionService.CreateAsync(options, cancellationToken: ct);

        return new CreatePaymentResponse(
            NonEmptyString.Create(session.Id).Value,
            new Uri(session.Url)
        );
    }
}
