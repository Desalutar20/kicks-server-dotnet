using Domain.Abstractions;
using Domain.Shared.ValueObjects;

namespace Domain.Products.ProductSkus;

public sealed record ProductSkuPrice
{
    public Money Price { get; }
    public Money? SalePrice { get; }

    public Money CurrentPrice => SalePrice ?? Price;

    private ProductSkuPrice(Money price, Money? salePrice)
    {
        Price = price;
        SalePrice = salePrice;
    }

    public static Result<ProductSkuPrice> Create(Money price, Money? salePrice)
    {
        if (price.Cents <= 0)
        {
            return Error.Validation("price", ["Price must be greater than zero."]);
        }

        if (salePrice is not null && salePrice.Cents <= 0)
        {
            return Error.Validation("salePrice", ["Sale price must be greater than zero."]);
        }

        if (salePrice is not null && salePrice >= price)
        {
            return Error.Validation(
                "salePrice",
                ["Sale price must be less than the regular price."]
            );
        }

        return new ProductSkuPrice(price, salePrice);
    }
};
