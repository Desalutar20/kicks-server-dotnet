using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Product.ProductSku;

public sealed record ProductSkuPrice
{
    public PositiveInt Price { get; }
    public PositiveInt? SalePrice { get; }

    private ProductSkuPrice(PositiveInt price, PositiveInt? salePrice)
    {
        Price = price;
        SalePrice = salePrice;
    }

    public static Result<ProductSkuPrice> Create(PositiveInt price, PositiveInt? salePrice)
    {
        if (salePrice is not null && salePrice >= price)
        {
            return Result<ProductSkuPrice>.Failure(
                Error.Validation("salePrice", ["Sale price must be less than the regular price."])
            );
        }

        return Result<ProductSkuPrice>.Success(new ProductSkuPrice(price, salePrice));
    }
};
