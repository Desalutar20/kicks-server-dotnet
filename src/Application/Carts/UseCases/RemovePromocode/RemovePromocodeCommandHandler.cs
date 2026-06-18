using Application.Abstractions.Database;
using Application.Carts.Errors;
using Domain.Carts;

namespace Application.Carts.UseCases.RemovePromocode;

public sealed record RemovePromocodeCommand(UserId UserId) : ICommand;

internal sealed class RemovePromocodeCommandHandler(
    ICartRepository cartRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<RemovePromocodeCommand>
{
    public async Task<Result> Handle(RemovePromocodeCommand command, CancellationToken ct = default)
    {
        var cart = await cartRepository.GetCartByUserIdAsync(command.UserId, true, ct);
        if (cart is null)
        {
            return CartErrors.CartNotFound;
        }

        if (cart.Promocode is null)
        {
            return Result.Success();
        }

        cart.RemovePromocode();

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
