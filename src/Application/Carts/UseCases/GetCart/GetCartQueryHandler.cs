using Application.Carts.Errors;
using Domain.Carts;

namespace Application.Carts.UseCases.GetCart;

public sealed record GetCartQuery(UserId UserId) : IQuery<Cart>;

internal sealed class GetCartQueryHandler(ICartRepository cartRepository)
    : IQueryHandler<GetCartQuery, Cart>
{
    public async Task<Result<Cart>> Handle(GetCartQuery query, CancellationToken ct = default)
    {
        var cart = await cartRepository.GetCartByUserIdAsync(query.UserId, false, ct);
        if (cart is null)
        {
            return CartErrors.CartNotFound;
        }

        return cart;
    }
}
