using Application.Abstractions.Database;
using Application.Admin.DeliveryOptions.Constants;
using Application.Admin.DeliveryOptions.Errors;
using Domain.DeliveryOptions;

namespace Application.Admin.DeliveryOptions.UseCases.DeleteDeliveryOption;

public sealed record DeleteDeliveryOptionCommand(DeliveryOptionId DeliveryOptionId) : ICommand;

internal sealed class DeleteDeliveryOptionCommandHandler(
    ICachingService cachingService,
    IDeliveryOptionRepository deliveryOptionRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<DeleteDeliveryOptionCommand>
{
    public async Task<Result> Handle(
        DeleteDeliveryOptionCommand command,
        CancellationToken ct = default
    )
    {
        var deliveryOption = await deliveryOptionRepository.GetDeliveryOptionByIdAsync(
            command.DeliveryOptionId,
            false,
            ct
        );
        if (deliveryOption is null)
        {
            return AdminDeliveryOptionsErrors.DeliveryOptionNotFound(command.DeliveryOptionId);
        }

        deliveryOptionRepository.DeleteDeliveryOption(deliveryOption);

        await cachingService.DeleteAsync(DeliveryOptionsConstants.CacheKey, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
