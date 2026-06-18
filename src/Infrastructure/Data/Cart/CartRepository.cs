using Domain.Carts;
using Domain.Promocodes;

namespace Infrastructure.Data.Cart;

internal sealed class CartRepository(AppDbContext dbContext)
    : RepositoryBase<DomainCart>(dbContext),
        ICartRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<DomainCart?> GetCartByUserIdAsync(
        UserId userId,
        bool trackChanges,
        CancellationToken ct = default
    ) => await FindByCondition(x => x.UserId == userId, trackChanges).FirstOrDefaultAsync(ct);

    public async Task RemovePromocodesFromCartsAsync(
        IEnumerable<PromocodeId> promocodeIds,
        CancellationToken ct = default
    ) =>
        await _dbContext
            .Carts.Where(c => c.PromocodeId != null && promocodeIds.Contains(c.PromocodeId))
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(c => c.PromocodeId, (PromocodeId?)null),
                ct
            );

    public void CreateCart(DomainCart cart) => Create(cart);
}
