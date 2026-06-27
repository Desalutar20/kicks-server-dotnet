using Application.Carts.Errors;
using Application.Carts.Types;

namespace Application.Carts.UseCases.GetCart;

public sealed record GetCartQuery(UserId UserId) : IQuery<CartResponse>;

internal sealed class GetCartQueryHandler(ICartReadRepository cartReadRepository)
    : IQueryHandler<GetCartQuery, CartResponse>
{
    public async Task<Result<CartResponse>> Handle(
        GetCartQuery query,
        CancellationToken ct = default
    )
    {
        var cart = await cartReadRepository.GetCartByUserIdAsync(query.UserId, ct);
        if (cart is null)
        {
            return CartErrors.CartNotFound;
        }

        return cart;
    }
}
