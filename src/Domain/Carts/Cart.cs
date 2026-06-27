using Domain.Abstractions;
using Domain.Products.ProductSkus;
using Domain.Promocodes;
using Domain.Shared.ValueObjects;
using Domain.Users;

namespace Domain.Carts;

public sealed class Cart(UserId userId) : Entity<CartId>(new CartId(Guid.NewGuid()))
{
    public const int MaxCartItemsLength = 100;

    public UserId UserId { get; private set; } = userId;
    public PromocodeId? PromocodeId { get; private set; }
    public Promocode? Promocode { get; private set; }

    private readonly List<CartItem> _cartItems = [];
    public IReadOnlyList<CartItem> CartItems => _cartItems;

    public Result AddProduct(ProductSkuId productSkuId)
    {
        var quantity = PositiveInt.Create(1).Value;
        var existingItem = _cartItems.FirstOrDefault(item => item.ProductSkuId == productSkuId);
        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);

            return Result.Success();
        }

        if (_cartItems.Count == MaxCartItemsLength)
        {
            return Error.Failure($"Cart cannot contain more than {MaxCartItemsLength} items.");
        }

        var cartItem = new CartItem(quantity, productSkuId);
        _cartItems.Add(cartItem);

        return Result.Success();
    }

    public void RemoveProduct(ProductSkuId productSkuId)
    {
        var item = _cartItems.FirstOrDefault(x => x.ProductSkuId == productSkuId);
        if (item is null)
        {
            return;
        }

        _cartItems.Remove(item);

        if (_cartItems.Count == 0)
        {
            PromocodeId = null;
        }
    }

    public Result UpdateProductQuantity(ProductSkuId productSkuId, PositiveInt quantity)
    {
        var item = _cartItems.FirstOrDefault(x => x.ProductSkuId == productSkuId);
        if (item is null)
        {
            return Result.Failure(Error.Failure("Cart item not found"));
        }

        item.UpdateQuantity(quantity);

        return Result.Success();
    }

    public Result ApplyPromocode(PromocodeId promocodeId)
    {
        if (_cartItems.Count == 0)
        {
            return Error.Failure("Cannot apply promocode to an empty cart.");
        }

        if (PromocodeId is not null)
        {
            return Error.Failure("Promocode is already applied.");
        }

        PromocodeId = promocodeId;

        return Result.Success();
    }

    public (Money TotalPrice, Money? DiscountedPrice) TotalPrice
    {
        get
        {
            var subtotalCents = _cartItems.Sum(x =>
                (x.ProductSku.Price.SalePrice ?? x.ProductSku.Price.Price).Cents * x.Quantity
            );

            var subtotal = Money.FromCents(subtotalCents).Value;

            if (Promocode is null)
                return (subtotal, null);

            var discount = Promocode.CalculateDiscount(subtotal);

            var final = subtotal - discount;

            return (subtotal, final);
        }
    }

    public void RemovePromocode()
    {
        PromocodeId = null;
        Promocode = null;
    }

    public void Clear()
    {
        _cartItems.Clear();
        PromocodeId = null;
        Promocode = null;
    }
}
