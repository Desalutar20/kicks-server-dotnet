using Application.Orders.Errors;
using Domain.Orders;

namespace Application.Orders.UseCases.GetOrder;

public sealed record GetOrderQuery(UserId UserId, OrderId OrderId) : IQuery<Order>;

internal sealed class GetOrderQueryHandler(IOrderRepository orderRepository)
    : IQueryHandler<GetOrderQuery, Order>
{
    public async Task<Result<Order>> Handle(GetOrderQuery query, CancellationToken ct = default)
    {
        var order = await orderRepository.GetOrderByUserIdAsync(
            query.UserId,
            query.OrderId,
            false,
            ct
        );

        if (order is null)
        {
            return OrderErrors.OrderNotFound;
        }

        return order;
    }
}
