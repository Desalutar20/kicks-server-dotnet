using Application.Abstractions.Database;
using Application.Carts.Errors;
using Domain.Carts;

namespace Application.Carts.UseCases.RemoveCartItem;

public sealed record RemoveCartItemCommand(UserId UserId, ProductSkuId ProductSkuId) : ICommand;

internal sealed class RemoveCartItemCommandHandler(
    IUnitOfWork unitOfWork,
    ICartRepository cartRepository
) : ICommandHandler<RemoveCartItemCommand>
{
    public async Task<Result> Handle(RemoveCartItemCommand command, CancellationToken ct = default)
    {
        var cart = await cartRepository.GetCartByUserIdAsync(command.UserId, true, ct);
        if (cart is null)
        {
            return CartErrors.CartNotFound;
        }

        if (cart.Promocode is not null && !cart.Promocode.IsValid)
        {
            cart.RemovePromocode();
        }

        cart.RemoveProduct(command.ProductSkuId);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
