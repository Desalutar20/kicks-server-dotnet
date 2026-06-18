using Domain.Promocodes;
using Domain.Shared.Pagination;
using Domain.Shared.ValueObjects;
using Domain.Users;

namespace Domain.Orders;

public interface IOrderRepository
{
    Task<Order?> GetOrderByUserIdAsync(
        UserId userId,
        OrderId orderId,
        bool trackChanges,
        CancellationToken ct = default
    );

    Task<KeysetPaginated<Order, OrderId>> GetOrdersByUserIdAsync(
        UserId userId,
        KeysetPagination<OrderId> keysetPagination,
        bool trackChanges,
        CancellationToken ct = default
    );

    Task<bool> IsPromocodeUsedByUserAsync(
        UserId userId,
        PromocodeId promocodeId,
        CancellationToken ct = default
    );

    Task<IEnumerable<ExpiredOrderItem>> GetAndLockExpiredOrdersAsync(
        PositiveInt batchSize,
        CancellationToken ct = default
    );

    Task BulkUpdateOrdersStatusAsync(
        IReadOnlyCollection<OrderId> ids,
        OrderStatus status,
        CancellationToken ct = default
    );

    void CreateOrder(Order order);
}
