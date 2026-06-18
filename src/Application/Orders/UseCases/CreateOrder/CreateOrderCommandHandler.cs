using Application.Abstractions.Database;
using Application.Carts.Errors;
using Domain.Carts;
using Domain.DeliveryOptions;
using Domain.Orders;
using Domain.Orders.Exceptions;
using Domain.Shared.ValueObjects;

namespace Application.Orders.UseCases.CreateOrder;

public sealed record CreateOrderCommand(
    UserId UserId,
    Email Email,
    NonEmptyString PhoneNumber,
    OrderAddress? BillingAddress,
    OrderAddress DeliveryAddress,
    DeliveryOptionId DeliveryOptionId
) : ICommand;

public class CreateOrderCommandHandler(
    ICartRepository cartRepository,
    IOrderRepository orderRepository,
    IDeliveryOptionRepository deliveryOptionRepository,
    IUnitOfWork unitOfWork,
    Config.Config config
) : ICommandHandler<CreateOrderCommand>
{
    public async Task<Result> Handle(CreateOrderCommand command, CancellationToken ct = default)
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
            return CartErrors.DeliveryOptionNotFound;
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
            return Error.Failure("Order creation limit exceeded.");
        }

        if (orders.Data.Count > 0 && orders.Data[0].Status == OrderStatus.Pending)
        {
            return Error.Failure(
                "You already have a pending order. Complete or cancel it before creating a new one."
            );
        }

        var orderItems = new List<OrderItem>();

        foreach (var ci in cart.CartItems)
        {
            var quantity = ci.FinalQuantity;
            var result = ci.ProductSku.DecreaseQuantity(quantity);
            if (result.IsFailure)
                return result.Error;

            orderItems.Add(
                new OrderItem(quantity, ci.ProductSku.Price.CurrentPrice, ci.ProductSkuId)
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

            return Result.Success();
        }
        catch (DeliveryOptionDoesNotExistsException)
        {
            return CartErrors.DeliveryOptionNotFound;
        }
    }
}
