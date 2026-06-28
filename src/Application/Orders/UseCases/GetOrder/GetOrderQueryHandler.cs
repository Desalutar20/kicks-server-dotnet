using Application.Admin.Orders;
using Application.Orders.Errors;
using Application.Orders.Types;
using Domain.Orders;

namespace Application.Orders.UseCases.GetOrder;

public sealed record GetOrderQuery(UserId UserId, OrderId OrderId) : IQuery<OrderResponse>;

internal sealed class GetOrderQueryHandler(IOrderReadRepository orderReadRepository)
    : IQueryHandler<GetOrderQuery, OrderResponse>
{
    public async Task<Result<OrderResponse>> Handle(
        GetOrderQuery query,
        CancellationToken ct = default
    )
    {
        var order = await orderReadRepository.GetOrderByUserIdAsync(
            query.UserId,
            query.OrderId,
            ct
        );

        if (order is null)
        {
            return OrderErrors.OrderNotFound;
        }

        return order;
    }
}
