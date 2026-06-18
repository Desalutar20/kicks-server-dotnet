using Domain.Abstractions;
using Domain.Shared.ValueObjects;

namespace Domain.DeliveryOptions;

public class DeliveryOption(
    DeliveryOptionTitle title,
    DeliveryOptionDescription description,
    Money price
) : Entity<DeliveryOptionId>(new DeliveryOptionId(Guid.NewGuid()))
{
    public DeliveryOptionTitle Title { get; private set; } = title;
    public DeliveryOptionDescription Description { get; private set; } = description;
    public Money Price { get; private set; } = price;

    public void Update(
        DeliveryOptionTitle title,
        DeliveryOptionDescription description,
        Money price
    )
    {
        Title = title;
        Description = description;
        Price = price;
    }
}
