using Application.Abstractions.Database;
using Application.Admin.DeliveryOptions.Constants;
using Application.Admin.DeliveryOptions.Errors;
using Application.Admin.DeliveryOptions.Types;
using Domain.DeliveryOptions;
using Domain.Shared.ValueObjects;

namespace Application.Admin.DeliveryOptions.UseCases.CreateDeliveryOption;

public sealed record CreateDeliveryOptionCommand(
    DeliveryOptionTitle Title,
    DeliveryOptionDescription Description,
    Money Price
) : ICommand<DeliveryOptionResponse>;

internal sealed class CreateDeliveryOptionCommandHandler(
    ICachingService cachingService,
    IDeliveryOptionRepository deliveryOptionRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateDeliveryOptionCommand, DeliveryOptionResponse>
{
    public async Task<Result<DeliveryOptionResponse>> Handle(
        CreateDeliveryOptionCommand command,
        CancellationToken ct = default
    )
    {
        var deliveryOption = await deliveryOptionRepository.GetDeliveryOptionByTitleAsync(
            command.Title,
            false,
            ct
        );

        if (deliveryOption is not null)
        {
            return AdminDeliveryOptionsErrors.DeliveryOptionAlreadyExists(command.Title);
        }

        var newDeliveryOption = new DeliveryOption(
            command.Title,
            command.Description,
            command.Price
        );
        deliveryOptionRepository.CreateDeliveryOption(newDeliveryOption);

        await cachingService.DeleteAsync(DeliveryOptionsConstants.CacheKey, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return newDeliveryOption.ToDto();
    }
}
