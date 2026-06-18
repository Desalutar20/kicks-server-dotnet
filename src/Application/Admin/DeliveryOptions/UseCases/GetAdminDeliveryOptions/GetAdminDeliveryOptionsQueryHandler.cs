using Domain.DeliveryOptions;

namespace Application.Admin.DeliveryOptions.UseCases.GetAdminDeliveryOptions;

public sealed record GetAdminDeliveryOptionsQuery : IQuery<IReadOnlyList<DeliveryOption>>;

internal sealed class GetAdminDeliveryOptionsQueryHandler(
    IDeliveryOptionRepository deliveryOptionRepository
) : IQueryHandler<GetAdminDeliveryOptionsQuery, IReadOnlyList<DeliveryOption>>
{
    public async Task<Result<IReadOnlyList<DeliveryOption>>> Handle(
        GetAdminDeliveryOptionsQuery query,
        CancellationToken ct = default
    )
    {
        var data = await deliveryOptionRepository.GetDeliveryOptionsAsync(false, ct);

        return Result<IReadOnlyList<DeliveryOption>>.Success(data);
    }
}
