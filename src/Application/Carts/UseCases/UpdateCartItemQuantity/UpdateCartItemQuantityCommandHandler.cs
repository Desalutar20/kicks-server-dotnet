using Application.Abstractions.Database;
using Application.Carts.Errors;
using Domain.Carts;
using Domain.Shared.ValueObjects;

namespace Application.Carts.UseCases.UpdateCartItemQuantity;

public sealed record UpdateCartItemQuantityCommand(
    UserId UserId,
    ProductSkuId ProductSkuId,
    PositiveInt Quantity
) : ICommand;

internal sealed class UpdateCartItemQuantityCommandHandler(
    IUnitOfWork unitOfWork,
    ICartRepository cartRepository
) : ICommandHandler<UpdateCartItemQuantityCommand>
{
    public async Task<Result> Handle(
        UpdateCartItemQuantityCommand command,
        CancellationToken ct = default
    )
    {
        var cart = await cartRepository.GetCartByUserIdAsync(command.UserId, true, ct);
        if (cart is null)
        {
            return CartErrors.CartNotFound;
        }

        var result = cart.UpdateProductQuantity(command.ProductSkuId, command.Quantity);
        if (result.IsFailure)
        {
            return result.Error;
        }

        if (cart.Promocode is not null && !cart.Promocode.IsValid)
        {
            cart.RemovePromocode();
        }

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
