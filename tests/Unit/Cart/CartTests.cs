using Domain.Products.ProductSkus;
using Domain.Promocodes;
using Domain.Shared;
using Domain.Shared.ValueObjects;
using Domain.Users;
using FluentAssertions;

namespace Unit.Cart;

public sealed class CartTests
{
    [Fact]
    public void AddProduct_Should_Add_New_Item()
    {
        var cart = CreateCart();
        var skuId = CreateSkuId();

        cart.AddProduct(skuId);

        cart.CartItems.Should().HaveCount(1);
        cart.CartItems[0].ProductSkuId.Should().Be(skuId);
        cart.CartItems[0].Quantity.Value.Should().Be(1);
    }

    [Fact]
    public void AddProduct_Should_Increase_Quantity_When_Product_Already_Exists()
    {
        var cart = CreateCart();
        var skuId = CreateSkuId();

        cart.AddProduct(skuId);
        cart.AddProduct(skuId);

        cart.CartItems.Should().HaveCount(1);
        cart.CartItems[0].Quantity.Value.Should().Be(2);
    }

    [Fact]
    public void AddProduct_Should_Return_Failure_When_Cart_Is_Full()
    {
        var cart = CreateCart();

        for (var i = 0; i < 100; i++)
        {
            var result = cart.AddProduct(new ProductSkuId(Guid.NewGuid()));

            result.IsSuccess.Should().BeTrue();
        }

        var overflowResult = cart.AddProduct(new ProductSkuId(Guid.NewGuid()));

        overflowResult.IsFailure.Should().BeTrue();
        cart.CartItems.Should().HaveCount(100);
    }

    [Fact]
    public void RemoveProduct_Should_Remove_Item()
    {
        var cart = CreateCart();
        var skuId = CreateSkuId();

        cart.AddProduct(skuId);

        cart.RemoveProduct(skuId);

        cart.CartItems.Should().BeEmpty();
    }

    [Fact]
    public void RemoveProduct_Should_Do_Nothing_When_Item_Does_Not_Exist()
    {
        var cart = CreateCart();

        var action = () => cart.RemoveProduct(CreateSkuId());

        action.Should().NotThrow();
        cart.CartItems.Should().BeEmpty();
    }

    [Fact]
    public void UpdateProductQuantity_Should_Update_Quantity()
    {
        var cart = CreateCart();
        var skuId = CreateSkuId();

        cart.AddProduct(skuId);

        var result = cart.UpdateProductQuantity(skuId, PositiveInt.Create(10).Value);

        result.IsSuccess.Should().BeTrue();
        cart.CartItems[0].Quantity.Value.Should().Be(10);
    }

    [Fact]
    public void UpdateProductQuantity_Should_Return_Failure_When_Item_Not_Found()
    {
        var cart = CreateCart();

        var result = cart.UpdateProductQuantity(CreateSkuId(), PositiveInt.Create(10).Value);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ApplyPromocode_Should_Return_Failure_For_Empty_Cart()
    {
        var cart = CreateCart();

        var result = cart.ApplyPromocode(CreatePromocodeId());

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ApplyPromocode_Should_Set_PromocodeId()
    {
        var cart = CreateCart();
        var skuId = CreateSkuId();
        var promocodeId = CreatePromocodeId();

        cart.AddProduct(skuId);

        var result = cart.ApplyPromocode(promocodeId);

        result.IsSuccess.Should().BeTrue();
        cart.PromocodeId.Should().Be(promocodeId);
    }

    [Fact]
    public void ApplyPromocode_Should_Return_Failure_When_Promocode_Already_Applied()
    {
        var cart = CreateCart();
        var skuId = CreateSkuId();

        cart.AddProduct(skuId);

        cart.ApplyPromocode(CreatePromocodeId());

        var result = cart.ApplyPromocode(CreatePromocodeId());

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void RemovePromocode_Should_Clear_Promocode()
    {
        var cart = CreateCart();
        var skuId = CreateSkuId();
        var promocodeId = CreatePromocodeId();

        cart.AddProduct(skuId);
        cart.ApplyPromocode(promocodeId);

        cart.RemovePromocode();

        cart.PromocodeId.Should().BeNull();
        cart.Promocode.Should().BeNull();
    }

    [Fact]
    public void Clear_Should_Remove_All_Items_And_Promocode()
    {
        var cart = CreateCart();

        cart.AddProduct(CreateSkuId());
        cart.AddProduct(CreateSkuId());

        cart.Clear();

        cart.CartItems.Should().BeEmpty();
        cart.PromocodeId.Should().BeNull();
        cart.Promocode.Should().BeNull();
    }

    [Fact]
    public void RemoveProduct_Should_Remove_Promocode_When_Last_Item_Removed()
    {
        var cart = CreateCart();
        var skuId = CreateSkuId();

        cart.AddProduct(skuId);
        cart.ApplyPromocode(CreatePromocodeId());

        cart.RemoveProduct(skuId);

        cart.CartItems.Should().BeEmpty();
        cart.PromocodeId.Should().BeNull();
    }

    private static Domain.Carts.Cart CreateCart() => new(new UserId(Guid.NewGuid()));

    private static ProductSkuId CreateSkuId() => new(Guid.NewGuid());

    private static PromocodeId CreatePromocodeId() => new(Guid.NewGuid());
}
