using Application.Carts.Types;

namespace Application.Carts;

public interface ICartReadRepository
{
    Task<CartResponse?> GetCartByUserIdAsync(UserId userId, CancellationToken ct = default);
}
