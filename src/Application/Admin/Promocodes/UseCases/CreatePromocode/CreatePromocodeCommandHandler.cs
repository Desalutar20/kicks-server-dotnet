using Application.Abstractions.Database;
using Application.Admin.Promocodes.Errors;
using Application.Admin.Promocodes.Types;
using Domain.Promocodes;
using Domain.Promocodes.Exceptions;
using Domain.Shared.ValueObjects;

namespace Application.Admin.Promocodes.UseCases.CreatePromocode;

public sealed record CreatePromocodeCommand(
    PositiveInt DiscountValue,
    PromocodeType Type,
    PromocodeValidityPeriod ValidityPeriod,
    PositiveInt UsageLimit,
    PromocodeCode Code
) : ICommand<AdminPromocodeResponse>;

internal sealed class CreatePromocodeCommandHandler(
    IPromocodeRepository promocodeRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreatePromocodeCommand, AdminPromocodeResponse>
{
    public async Task<Result<AdminPromocodeResponse>> Handle(
        CreatePromocodeCommand command,
        CancellationToken ct = default
    )
    {
        var promocode = Promocode.Create(
            command.DiscountValue,
            command.Type,
            command.ValidityPeriod,
            command.UsageLimit,
            command.Code
        );
        if (promocode.IsFailure)
        {
            return promocode.Error;
        }

        promocodeRepository.CreatePromocode(promocode.Value);

        try
        {
            await unitOfWork.SaveChangesAsync(ct);

            return promocode.Value.ToAdminResponse();
        }
        catch (PromocodeAlreadyExistsException)
        {
            return AdminPromocodeErrors.PromocodeAlreadyExists(command.Code);
        }
    }
}
