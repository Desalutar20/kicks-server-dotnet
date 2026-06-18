using Application.Abstractions.Database;
using Application.Carts.Errors;
using Domain.Carts;
using Domain.Carts.Exceptions;
using Domain.Shared.ValueObjects;

namespace Application.Carts.UseCases.AddCartItem;

public sealed record AddCartItemCommand(
    UserId UserId,
    ProductSkuId ProductSkuId,
    PositiveInt Quantity
) : ICommand;

internal sealed class AddCartItemCommandHandler(
    IUnitOfWork unitOfWork,
    ICartRepository cartRepository
) : ICommandHandler<AddCartItemCommand>
{
    public async Task<Result> Handle(AddCartItemCommand command, CancellationToken ct = default)
    {
        var cart = await cartRepository.GetCartByUserIdAsync(command.UserId, true, ct);
        if (cart is null)
        {
            return CartErrors.CartNotFound;
        }

        var result = cart.AddProduct(command.ProductSkuId, command.Quantity);
        if (result.IsFailure)
        {
            return result.Error;
        }

        if (cart.Promocode is not null && !cart.Promocode.IsValid)
        {
            cart.RemovePromocode();
        }

        try
        {
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (ProductSkuDoesNotExistsException)
        {
            return CartErrors.ProductSkuNotFound;
        }
    }
}
