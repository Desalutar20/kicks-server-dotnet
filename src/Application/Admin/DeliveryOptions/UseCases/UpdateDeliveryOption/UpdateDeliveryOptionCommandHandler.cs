using Application.Abstractions.Database;
using Application.Admin.DeliveryOptions.Constants;
using Application.Admin.DeliveryOptions.Errors;
using Domain.DeliveryOptions;
using Domain.Shared.ValueObjects;

namespace Application.Admin.DeliveryOptions.UseCases.UpdateDeliveryOption;

public sealed record UpdateDeliveryOptionCommand(
    DeliveryOptionId Id,
    DeliveryOptionTitle? Title,
    DeliveryOptionDescription? Description,
    Money? Price
) : ICommand;

internal sealed class UpdateDeliveryOptionCommandHandler(
    ICachingService cachingService,
    IDeliveryOptionRepository deliveryOptionRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateDeliveryOptionCommand>
{
    public async Task<Result> Handle(
        UpdateDeliveryOptionCommand command,
        CancellationToken ct = default
    )
    {
        var deliveryOption = await deliveryOptionRepository.GetDeliveryOptionByIdAsync(
            command.Id,
            true,
            ct
        );
        if (deliveryOption is null)
        {
            return AdminDeliveryOptionsErrors.DeliveryOptionNotFound(command.Id);
        }

        if (command.Title is not null)
        {
            var duplicateDeliveryOption =
                await deliveryOptionRepository.GetDeliveryOptionByTitleAsync(
                    command.Title,
                    false,
                    ct
                );

            if (duplicateDeliveryOption is not null)
            {
                return AdminDeliveryOptionsErrors.DeliveryOptionAlreadyExists(command.Title);
            }
        }

        deliveryOption.Update(
            command.Title ?? deliveryOption.Title,
            command.Description ?? deliveryOption.Description,
            command.Price ?? deliveryOption.Price
        );

        await cachingService.DeleteAsync(DeliveryOptionsConstants.CacheKey, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
