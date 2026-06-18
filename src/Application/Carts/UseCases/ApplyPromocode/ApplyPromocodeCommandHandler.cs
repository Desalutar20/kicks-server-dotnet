using Application.Abstractions.Database;
using Application.Carts.Errors;
using Domain.Carts;
using Domain.Orders;
using Domain.Promocodes;

namespace Application.Carts.UseCases.ApplyPromocode;

public sealed record ApplyPromocodeCommand(UserId UserId, PromocodeCode Code) : ICommand;

internal sealed class ApplyPromocodeCommandHandler(
    ICartRepository cartRepository,
    IOrderRepository orderRepository,
    IPromocodeRepository promocodeRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<ApplyPromocodeCommand>
{
    public async Task<Result> Handle(ApplyPromocodeCommand command, CancellationToken ct = default)
    {
        var cart = await cartRepository.GetCartByUserIdAsync(command.UserId, true, ct);
        if (cart is null)
        {
            return CartErrors.CartNotFound;
        }

        var promocode = await promocodeRepository.GetPromocodeByCodeAsync(command.Code, false, ct);
        if (promocode is null)
        {
            return CartErrors.InvalidPromocode;
        }

        if (await orderRepository.IsPromocodeUsedByUserAsync(command.UserId, promocode.Id, ct))
        {
            return CartErrors.PromocodeAlreadyUsed;
        }

        var result = cart.ApplyPromocode(promocode.Id);
        if (result.IsFailure)
        {
            return result.Error;
        }

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
