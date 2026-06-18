using Application.Abstractions.Database;
using Application.Admin.DeliveryOptions.Errors;
using Domain.DeliveryOptions;
using Domain.Shared.ValueObjects;

namespace Application.Admin.DeliveryOptions.UseCases.CreateDeliveryOption;

public sealed record CreateDeliveryOptionCommand(
    DeliveryOptionTitle Title,
    DeliveryOptionDescription Description,
    Money Price
) : ICommand<DeliveryOption>;

internal sealed class CreateDeliveryOptionCommandHandler(
    IDeliveryOptionRepository deliveryOptionRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateDeliveryOptionCommand, DeliveryOption>
{
    public async Task<Result<DeliveryOption>> Handle(
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

        await unitOfWork.SaveChangesAsync(ct);

        return newDeliveryOption;
    }
}
