using Domain.Promocodes;

namespace Domain.DeliveryOptions;

public interface IDeliveryOptionRepository
{
    Task<IReadOnlyList<DeliveryOption>> GetDeliveryOptionsAsync(
        bool trackChanges,
        CancellationToken ct = default
    );

    Task<DeliveryOption?> GetDeliveryOptionByIdAsync(
        DeliveryOptionId id,
        bool trackChanges,
        CancellationToken ct = default
    );

    Task<DeliveryOption?> GetDeliveryOptionByTitleAsync(
        DeliveryOptionTitle title,
        bool trackChanges,
        CancellationToken ct = default
    );

    void CreateDeliveryOption(DeliveryOption deliveryOption);
    void UpdateDeliveryOption(DeliveryOption deliveryOption);
    void DeleteDeliveryOption(DeliveryOption deliveryOption);
}
