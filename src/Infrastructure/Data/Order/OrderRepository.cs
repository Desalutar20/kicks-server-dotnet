using Domain.Orders;
using Domain.Promocodes;
using Domain.Shared.ValueObjects;
using Infrastructure.Data.Extensions;

namespace Infrastructure.Data.Order;

internal sealed class OrderRepository(AppDbContext dbContext)
    : RepositoryBase<DomainOrder>(dbContext),
        IOrderRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<DomainOrder?> GetOrderByUserIdAsync(
        UserId userId,
        OrderId orderId,
        bool trackChanges,
        CancellationToken ct = default
    ) =>
        await FindByCondition(x => x.UserId == userId && x.Id == orderId, trackChanges)
            .FirstOrDefaultAsync(ct);

    public async Task<KeysetPaginated<DomainOrder, OrderId>> GetOrdersByUserIdAsync(
        UserId userId,
        KeysetPagination<OrderId> keysetPagination,
        bool trackChanges,
        CancellationToken ct = default
    )
    {
        var query = _dbContext.Orders.AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        query = query
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ApplyKeysetPagination(keysetPagination);

        var result = await query.ToListAsync(ct);

        return new KeysetPaginated<DomainOrder, OrderId>(
            result,
            keysetPagination,
            u => u.CreatedAt,
            u => u.Id
        );
    }

    public void CreateOrder(DomainOrder order) => Create(order);

    public async Task<bool> IsPromocodeUsedByUserAsync(
        UserId userId,
        PromocodeId promocodeId,
        CancellationToken ct = default
    )
    {
        return await _dbContext.Orders.AnyAsync(
            o =>
                o.UserId == userId
                && o.PromocodeId == promocodeId
                && o.Status != OrderStatus.Cancelled,
            ct
        );
    }

    public async Task<IEnumerable<ExpiredOrderItem>> GetAndLockExpiredOrdersAsync(
        PositiveInt batchSize,
        CancellationToken ct = default
    )
    {
        var rows = await _dbContext
            .Database.SqlQuery<ExpiredOrderDbRow>(
                $"""
                SELECT id, promocode_id
                FROM "order"
                WHERE NOW() > expires_at
                  AND status = 'pending'
                ORDER BY created_at
                FOR UPDATE SKIP LOCKED
                LIMIT {batchSize.Value}
                """
            )
            .ToListAsync(ct);

        List<ExpiredOrderItem> items = [];

        foreach (var row in rows)
        {
            var orderItemsDb = await _dbContext
                .Database.SqlQuery<ExpiredOrderItemDbRow>(
                    $"""
                    SELECT quantity, product_sku_id
                    FROM order_item
                    WHERE order_id = {row.Id}
                    """
                )
                .ToListAsync(ct);

            var orderItems = orderItemsDb
                .Select(x =>
                    (new ProductSkuId(x.ProductSkuId), PositiveInt.Create(x.Quantity).Value)
                )
                .ToList();

            items.Add(
                new ExpiredOrderItem(
                    new OrderId(row.Id),
                    row.PromocodeId is not null ? new PromocodeId(row.PromocodeId.Value) : null,
                    orderItems
                )
            );
        }

        return items;
    }

    public async Task BulkUpdateOrdersStatusAsync(
        IReadOnlyCollection<OrderId> ids,
        OrderStatus status,
        CancellationToken ct = default
    )
    {
        await _dbContext
            .Orders.Where(x => ids.Contains(x.Id))
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(x => x.Status, status)
                        .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow),
                ct
            );
    }
}
