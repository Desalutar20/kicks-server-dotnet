using Application.Orders.Types;
using Domain.Orders;
using Domain.Promocodes;

namespace Application.Admin.Orders;

public interface IOrderReadRepository
{
    Task<OrderResponse?> GetOrderByUserIdAsync(
        UserId userId,
        OrderId orderId,
        CancellationToken ct = default
    );

    Task<KeysetPaginated<OrderResponse, Guid>> GetOrdersByUserIdAsync(
        UserId userId,
        KeysetPagination<Guid> keysetPagination,
        CancellationToken ct = default
    );

    Task<bool> IsPromocodeUsedByUserAsync(
        UserId userId,
        PromocodeId promocodeId,
        CancellationToken ct = default
    );
}
