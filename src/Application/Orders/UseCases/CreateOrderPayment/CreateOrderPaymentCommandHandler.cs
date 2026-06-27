using Application.Abstractions.Database;
using Application.Abstractions.Payment;
using Application.Orders.Errors;
using Domain.Orders;
using Domain.Shared.ValueObjects;

namespace Application.Orders.UseCases.CreateOrderPayment;

public sealed record CreateOrderPaymentCommand(UserId UserId, OrderId OrderId) : ICommand<Uri>;

internal sealed class CreateOrderPaymentCommandHandler(
    Config.Config config,
    IPaymentService paymentService,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateOrderPaymentCommand, Uri>
{
    public async Task<Result<Uri>> Handle(
        CreateOrderPaymentCommand command,
        CancellationToken ct = default
    )
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        var order = await orderRepository.GetAndLockOrderByUserIdAsync(
            command.UserId,
            command.OrderId,
            ct
        );
        if (order is null)
        {
            return OrderErrors.OrderNotFound;
        }

        if (order.IsExpired)
        {
            return OrderErrors.OrderExpired;
        }

        var result = order.ExtendExpiration(
            PositiveInt.Create(config.Application.OrderExpirationTtlMinutes).Value
        );
        if (result.IsFailure)
        {
            return result.Error;
        }

        var items = new List<PaymentLineItem>();

        items.AddRange(
            order.OrderItems.Select(x => new PaymentLineItem(
                NonEmptyString.Create(x.ProductSku.Product.Title).Value,
                x.Price,
                x.Quantity,
                NonEmptyString.Create(x.ProductSku.Product.Description).Value,
                x.ProductSku.Images.Select(i => i.Url.Value).ToList()
            ))
        );

        if (order.DeliveryPrice.Cents > 0)
        {
            items.Add(
                new PaymentLineItem(
                    NonEmptyString.Create("Delivery").Value,
                    order.DeliveryPrice,
                    PositiveInt.Create(1).Value,
                    null,
                    []
                )
            );
        }

        var paymentRequest = new CreatePaymentRequest(
            order.Email,
            NonEmptyString.Create("usd").Value,
            order.Total,
            items,
            new Uri("https://google.com"),
            new Uri("https://google.com"),
            new Dictionary<string, string> { ["order_id"] = order.Id.Value.ToString() }
        );

        var paymentResult = await paymentService.CreatePaymentAsync(paymentRequest, ct);

        var payment = OrderPayment.Create(
            OrderPaymentTransactionId.Create(paymentResult.PaymentId.Value).Value,
            order.Total
        );

        var addPaymentResult = order.AddPayment(payment.Value);
        if (addPaymentResult.IsFailure)
        {
            return addPaymentResult.Error;
        }

        await unitOfWork.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return paymentResult.CheckoutUrl;
    }
}
