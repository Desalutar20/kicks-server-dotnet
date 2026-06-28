using Application.Orders.Types;
using Domain.Orders;

namespace Application.Orders.UseCases.GetOrders;

public sealed record GetOrdersQuery(UserId UserId, KeysetPagination<Guid> KeysetPagination)
    : IQuery<KeysetPaginated<OrderResponse, Guid>>;

internal sealed class GetOrdersQueryHandler()
    : IQueryHandler<GetOrdersQuery, KeysetPaginated<OrderResponse, Guid>>
{
    public async Task<Result<KeysetPaginated<OrderResponse, Guid>>> Handle(
        GetOrdersQuery query,
        CancellationToken ct = default
    )
    {
        //TODO
        // var orders = await orderRepository.GetOrdersByUserIdAsync(
        //     query.UserId,
        //     query.KeysetPagination,
        //     false,
        //     ct
        // );

        // return orders;

        throw new NotImplementedException();
    }
}
