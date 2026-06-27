using Application.Abstractions.Database;
using Application.Carts.Errors;
using Application.Orders.Errors;
using Domain.Carts;
using Domain.DeliveryOptions;
using Domain.Orders;
using Domain.Orders.Exceptions;
using Domain.Shared.ValueObjects;

namespace Application.Orders.UseCases.CreateOrder;

public sealed record CreateOrderCommand(
    UserId UserId,
    Email Email,
    PhoneNumber PhoneNumber,
    OrderAddress? BillingAddress,
    OrderAddress DeliveryAddress,
    DeliveryOptionId DeliveryOptionId
) : ICommand<OrderId>;

public class CreateOrderCommandHandler(
    ICartRepository cartRepository,
    IOrderRepository orderRepository,
    IDeliveryOptionRepository deliveryOptionRepository,
    IUnitOfWork unitOfWork,
    Config.Config config
) : ICommandHandler<CreateOrderCommand, OrderId>
{
    public async Task<Result<OrderId>> Handle(
        CreateOrderCommand command,
        CancellationToken ct = default
    )
    {
        var cart = await cartRepository.GetCartByUserIdAsync(command.UserId, true, ct);
        if (cart is null)
            return CartErrors.CartNotFound;

        if (cart.Promocode is not null && !cart.Promocode.IsValid)
        {
            cart.RemovePromocode();
            await unitOfWork.SaveChangesAsync(ct);
            return CartErrors.InvalidPromocode;
        }

        var deliveryOption = await deliveryOptionRepository.GetDeliveryOptionByIdAsync(
            command.DeliveryOptionId,
            false,
            ct
        );
        if (deliveryOption is null)
        {
            return OrderErrors.DeliveryOptionNotFound;
        }

        var maxCancelledOrdersPerDay = config.Application.MaxCancelledOrdersPerDay;
        var orders = await orderRepository.GetOrdersByUserIdAsync(
            command.UserId,
            new KeysetPagination<OrderId>(
                PositiveInt.Create(maxCancelledOrdersPerDay).Value,
                null,
                null
            ),
            false,
            ct
        );

        var cancelledOrders = orders
            .Data.Where(x => x.Status == OrderStatus.Cancelled)
            .OrderByDescending(x => x.CreatedAt)
            .Take(maxCancelledOrdersPerDay)
            .ToList();

        if (
            cancelledOrders.Count == maxCancelledOrdersPerDay
            && cancelledOrders.Last().CreatedAt > DateTimeOffset.UtcNow.AddDays(-1)
        )
        {
            return OrderErrors.OrderCreationLimitExceeded;
        }

        if (orders.Data.Any(x => x.Status == OrderStatus.Pending))
        {
            return OrderErrors.AlreadyHasPendingError;
        }

        var orderItems = new List<OrderItem>();

        foreach (var ci in cart.CartItems)
        {
            var quantity = ci.FinalQuantity;
            if (quantity == 0)
                continue;

            var positiveQuantity = PositiveInt.Create(quantity).Value;

            var result = ci.ProductSku.DecreaseQuantity(positiveQuantity);
            if (result.IsFailure)
                return result.Error;

            orderItems.Add(
                new OrderItem(positiveQuantity, ci.ProductSku.Price.CurrentPrice, ci.ProductSkuId)
            );
        }

        var order = Order.Create(
            command.Email,
            command.PhoneNumber,
            command.BillingAddress,
            command.DeliveryAddress,
            DateTimeOffset.UtcNow.AddMinutes(config.Application.OrderExpirationTtlMinutes),
            orderItems,
            command.UserId,
            deliveryOption,
            cart.Promocode
        );

        if (order.IsFailure)
            return order.Error;

        orderRepository.CreateOrder(order.Value);

        if (order.Value.Promocode is not null)
        {
            var increasePromocodeUsageCountResult = order.Value.Promocode.IncreaseUsageCount();
            if (increasePromocodeUsageCountResult.IsFailure)
            {
                return increasePromocodeUsageCountResult.Error;
            }
        }

        try
        {
            await unitOfWork.SaveChangesAsync(ct);

            return order.Value.Id;
        }
        catch (DeliveryOptionDoesNotExistsException)
        {
            return OrderErrors.DeliveryOptionNotFound;
        }
        catch (PendingOrderAlreadyExistsException)
        {
            return OrderErrors.AlreadyHasPendingError;
        }
    }
}
