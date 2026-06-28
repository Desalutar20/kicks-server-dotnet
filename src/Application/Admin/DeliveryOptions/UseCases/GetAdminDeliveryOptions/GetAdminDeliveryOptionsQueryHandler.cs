using System.Text.Json;
using Application.Admin.DeliveryOptions.Constants;
using Application.Admin.DeliveryOptions.JsonConverters;
using Application.Admin.DeliveryOptions.Types;
using Domain.DeliveryOptions;
using Domain.Shared.ValueObjects;

namespace Application.Admin.DeliveryOptions.UseCases.GetAdminDeliveryOptions;

public sealed record GetAdminDeliveryOptionsQuery
    : IQuery<IReadOnlyList<AdminDeliveryOptionResponse>>;

internal sealed class GetAdminDeliveryOptionsQueryHandler(
    ICachingService cachingService,
    IDeliveryOptionRepository deliveryOptionRepository
) : IQueryHandler<GetAdminDeliveryOptionsQuery, IReadOnlyList<AdminDeliveryOptionResponse>>
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new DeliveryOptionResponseConverter() },
    };

    public async Task<Result<IReadOnlyList<AdminDeliveryOptionResponse>>> Handle(
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
            var deserialized = JsonSerializer.Deserialize<
                IReadOnlyList<AdminDeliveryOptionResponse>
            >(fromCache, Options);

            return deserialized!.ToList();
        }

        var data = await deliveryOptionRepository.GetDeliveryOptionsAsync(false, ct);
        var mapped = data.Select(x => x.ToAdminResponse()).ToList();
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
