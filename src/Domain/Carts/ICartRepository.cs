using Domain.Promocodes;
using Domain.Users;

namespace Domain.Carts;

public interface ICartRepository
{
    Task<Cart?> GetCartByUserIdAsync(
        UserId userId,
        bool trackChanges,
        CancellationToken ct = default
    );

    Task RemovePromocodesFromCartsAsync(
        IEnumerable<PromocodeId> promocodeIds,
        CancellationToken ct = default
    );

    void CreateCart(Cart cart);
}
