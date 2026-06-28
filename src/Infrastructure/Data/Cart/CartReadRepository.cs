using System.Data;
using Application.Carts;
using Application.Carts.Types;
using Dapper;

namespace Infrastructure.Data.Cart;

internal sealed class CartReadRepository(IDbConnection connection) : ICartReadRepository
{
    public async Task<CartResponse?> GetCartByUserIdAsync(
        UserId userId,
        CancellationToken ct = default
    )
    {
        const string sql = """
            SELECT
                   COALESCE(
                     JSON_AGG(
                        JSON_BUILD_OBJECT(
                            'Id', ps.id,
                            'Price', ps.price / 100.0,
                            'SalePrice', ps.sale_price / 100.0,
                            'Title', p.title,
                            'Description', p.description,
                            'Size', ps.size,
                            'Quantity',
                                   CASE
                                        WHEN ps.quantity < ci.quantity THEN ps.quantity
                                        ELSE ci.quantity
                                    END,
                            'Image', ps.images->0->>'Url'
                        )
                    ) FILTER (WHERE ps.id IS NOT NULL), '[]') as Items,
                (
                  SUM(
                          LEAST(ps.quantity, ci.quantity)
                          * COALESCE(ps.sale_price, ps.price)
                      ) / 100.0
                  )::numeric(10,2) AS TotalPrice,
                   promocode.discount_value as DiscountValue,
                   promocode.type as Type,
                   promocode.valid_to as ValidTo,
                   promocode.code as Code
            FROM cart c
            LEFT JOIN cart_item ci ON c.id = ci.cart_id
            LEFT JOIN product_sku ps ON ci.product_sku_id = ps.id
            LEFT JOIN product p ON ps.product_id = p.id
            LEFT JOIN promocode ON c.promocode_id = promocode.id
            WHERE user_id = @UserId
            GROUP BY
                promocode.discount_value,
                promocode.type,
                promocode.valid_to,
                promocode.code;
            """;

        var command = new CommandDefinition(
            sql,
            new { UserId = userId.Value },
            cancellationToken: ct
        );

        var cart = await connection.QueryAsync<CartResponse, PromocodeResponse, CartResponse>(
            command,
            (cart, promocode) => cart with { Promocode = promocode },
            splitOn: "DiscountValue"
        );

        return cart.FirstOrDefault();
    }
}
