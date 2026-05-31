using Application.Abstractions.Database;
using Application.Admin.Promocodes.Errors;
using Domain.Promocodes;
using Domain.Promocodes.Exceptions;

namespace Application.Admin.Promocodes.UseCases.CreatePromocode;

public sealed record CreatePromocodeCommand(
    PositiveInt DiscountValue,
    PromocodeType Type,
    PromocodeValidityPeriod ValidityPeriod,
    PositiveInt UsageLimit,
    PromocodeCode Code
) : ICommand<Promocode>;

internal sealed class CreatePromocodeCommandHandler(
    IPromocodeRepository promocodeRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreatePromocodeCommand, Promocode>
{
    public async Task<Result<Promocode>> Handle(
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

            return promocode;
        }
        catch (PromocodeAlreadyExistsException)
        {
            return AdminPromocodeErrors.PromocodeAlreadyExists(command.Code);
        }
    }
}
