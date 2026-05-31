using Application.Abstractions.Database;
using Application.Admin.Promocodes.Errors;
using Domain.Promocodes;

namespace Application.Admin.Promocodes.UseCases.DeletePromocode;

public sealed record DeletePromocodeCommand(PromocodeId PromocodeId) : ICommand;

internal sealed class DeletePromocodeCommandHandler(
    IPromocodeRepository promocodeRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<DeletePromocodeCommand>
{
    public async Task<Result> Handle(DeletePromocodeCommand command, CancellationToken ct = default)
    {
        var promocode = await promocodeRepository.GetPromocodeByIdAsync(
            command.PromocodeId,
            false,
            ct
        );
        if (promocode is null)
        {
            return AdminPromocodeErrors.PromocodeNotFound(command.PromocodeId);
        }

        promocodeRepository.DeletePromocode(promocode);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
