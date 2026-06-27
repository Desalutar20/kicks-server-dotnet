using System.Text.Json;
using Application.Admin.DeliveryOptions.Constants;
using Application.Admin.DeliveryOptions.JsonConverters;
using Application.Admin.DeliveryOptions.Types;
using Domain.DeliveryOptions;
using Domain.Shared.ValueObjects;

namespace Application.Admin.DeliveryOptions.UseCases.GetAdminDeliveryOptions;

public sealed record GetAdminDeliveryOptionsQuery : IQuery<IReadOnlyList<DeliveryOptionResponse>>;

internal sealed class GetAdminDeliveryOptionsQueryHandler(
    ICachingService cachingService,
    IDeliveryOptionRepository deliveryOptionRepository
) : IQueryHandler<GetAdminDeliveryOptionsQuery, IReadOnlyList<DeliveryOptionResponse>>
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new DeliveryOptionResponseConverter() },
    };

    public async Task<Result<IReadOnlyList<DeliveryOptionResponse>>> Handle(
        GetAdminDeliveryOptionsQuery query,
        CancellationToken ct = default
    )
    {
        var fromCache = await cachingService.GetAsync(
            DeliveryOptionsConstants.CacheKey,
            TimeSpan.FromHours(1),
            ct
        );
        if (fromCache is not null)
        {
            var deserialized = JsonSerializer.Deserialize<IReadOnlyList<DeliveryOptionResponse>>(
                fromCache,
                Options
            );

            return deserialized!.ToList();
        }

        var data = await deliveryOptionRepository.GetDeliveryOptionsAsync(false, ct);
        var mapped = data.Select(x => x.ToDto()).ToList();
        var serialized = JsonSerializer.Serialize(mapped, Options);

        await cachingService.SetAsync(
            DeliveryOptionsConstants.CacheKey,
            NonEmptyString.Create(serialized).Value,
            null,
            ct
        );

        return mapped;
    }
}
