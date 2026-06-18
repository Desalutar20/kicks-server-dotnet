using Application.Abstractions.Database;
using Application.Admin.Promocodes.Errors;
using Domain.Promocodes;
using Domain.Promocodes.Exceptions;
using Domain.Shared.ValueObjects;

namespace Application.Admin.Promocodes.UseCases.UpdatePromocode;

public sealed record UpdatePromocodeCommand(
    PromocodeId Id,
    PositiveInt? DiscountValue,
    PromocodeType? Type,
    PromocodeValidityPeriod? ValidityPeriod,
    PositiveInt? UsageLimit,
    PromocodeCode? Code
) : ICommand;

internal sealed class UpdatePromocodeCommandHandler(
    IPromocodeRepository promocodeRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdatePromocodeCommand>
{
    public async Task<Result> Handle(UpdatePromocodeCommand command, CancellationToken ct = default)
    {
        var promocode = await promocodeRepository.GetPromocodeByIdAsync(command.Id, true, ct);
        if (promocode is null)
        {
            return AdminPromocodeErrors.PromocodeNotFound(command.Id);
        }

        var result = promocode.Update(
            command.DiscountValue ?? promocode.DiscountValue,
            command.Type ?? promocode.Type,
            command.ValidityPeriod ?? promocode.ValidityPeriod,
            command.UsageLimit ?? promocode.UsageLimit,
            command.Code ?? promocode.Code
        );
        if (result.IsFailure)
        {
            return result.Error;
        }

        try
        {
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (PromocodeAlreadyExistsException)
        {
            return AdminPromocodeErrors.PromocodeAlreadyExists(command.Code ?? promocode.Code);
        }
    }
}
