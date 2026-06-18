using Domain.Orders;

namespace Application.Orders.UseCases.GetOrders;

public sealed record GetOrdersQuery(UserId UserId, KeysetPagination<OrderId> KeysetPagination)
    : IQuery<KeysetPaginated<Order, OrderId>>;

internal sealed class GetOrdersQueryHandler(IOrderRepository orderRepository)
    : IQueryHandler<GetOrdersQuery, KeysetPaginated<Order, OrderId>>
{
    public async Task<Result<KeysetPaginated<Order, OrderId>>> Handle(
        GetOrdersQuery query,
        CancellationToken ct = default
    )
    {
        var orders = await orderRepository.GetOrdersByUserIdAsync(
            query.UserId,
            query.KeysetPagination,
            false,
            ct
        );

        return orders;
    }
}
