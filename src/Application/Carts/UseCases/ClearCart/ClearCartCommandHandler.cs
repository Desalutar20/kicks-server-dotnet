using Application.Abstractions.Database;
using Application.Carts.Errors;
using Domain.Carts;

namespace Application.Carts.UseCases.ClearCart;

public sealed record ClearCartCommand(UserId UserId) : ICommand;

internal sealed class ClearCartCommandHandler(
    ICartRepository cartRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<ClearCartCommand>
{
    public async Task<Result> Handle(ClearCartCommand command, CancellationToken ct = default)
    {
        var cart = await cartRepository.GetCartByUserIdAsync(command.UserId, true, ct);
        if (cart is null)
        {
            return CartErrors.CartNotFound;
        }

        cart.Clear();

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
