using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Products.ProductSkus;

public sealed record ProductSkuPrice
{
    public PositiveInt Price { get; }
    public PositiveInt? SalePrice { get; }

    public PositiveInt CurrentPrice => SalePrice ?? Price;

    private ProductSkuPrice(PositiveInt price, PositiveInt? salePrice)
    {
        Price = price;
        SalePrice = salePrice;
    }

    public static Result<ProductSkuPrice> Create(PositiveInt price, PositiveInt? salePrice)
    {
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
