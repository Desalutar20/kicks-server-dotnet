using System.Data;
using Application.Admin.DeliveryOptions.Types;
using Application.Admin.Orders;
using Application.Carts.Types;
using Application.Orders.Types;
using Dapper;
using Domain.Orders;
using Domain.Promocodes;

namespace Infrastructure.Data.Order;

internal sealed class OrderReadRepository(IDbConnection connection) : IOrderReadRepository
{
    private const string BaseSelect = """
        SELECT
             o.id as Id,
                JSON_AGG(
                    JSON_BUILD_OBJECT(
                        'Id', oi.id,
                        'Price', oi.price / 100.0,
                        'Title', p.title,
                        'Description', p.description,
                        'Size', ps.size,
                        'Quantity', oi.quantity,
                        'Image', ps.images->0->>'Url'
                    )
             ) as Items,
             o.email as Email,
             o.phone_number as PhoneNumber,
             CASE
                  WHEN o.billing_address_city IS NULL
                    OR o.billing_address_street IS NULL
                    OR o.billing_address_home IS NULL
                    OR o.billing_address_apartment IS NULL
                  THEN NULL
                  ELSE JSON_BUILD_OBJECT(
                      'City', o.billing_address_city,
                      'Street', o.billing_address_street,
                      'Home', o.billing_address_home,
                      'Apartment', o.billing_address_apartment
                  )
             END AS BillingAddress,
             JSON_BUILD_OBJECT(
                    'City', o.delivery_address_city,
                    'Street', o.delivery_address_street,
                    'Home', o.delivery_address_home,
                    'Apartment', o.delivery_address_apartment
             ) AS DeliveryAddress,
             o.status as Status,
             o.expires_at as ExpiresAt,
             o.delivery_price / 100.0 as DeliveryPrice,
             (
                SUM(
                    oi.quantity * oi.price
                ) / 100.0
             )::numeric(10,2) AS TotalPrice,
             promocode.discount_value as DiscountValue,
             promocode.type as Type,
             promocode.valid_to as ValidTo,
             promocode.code as Code,
             d.id as Id,
             d.title as Title,
             d.description as Description,
             d.price / 100.0 as Price
        FROM "order" o
        JOIN order_item oi ON o.id = oi.order_id
        JOIN product_sku ps ON oi.product_sku_id = ps.id
        JOIN product p ON ps.product_id = p.id
        LEFT JOIN promocode ON o.promocode_id = promocode.id
        JOIN delivery_options d ON o.delivery_option_id = d.id
        """;

    public async Task<OrderResponse?> GetOrderByUserIdAsync(
        UserId userId,
        OrderId orderId,
        CancellationToken ct = default
    )
    {
        const string sql =
            BaseSelect
            + """

                WHERE o.user_id = @UserId AND o.id = @Id
                GROUP BY
                    o.id,
                    o.email,
                    o.phone_number,
                    o.status,
                    o.expires_at,
                    o.delivery_price,
                    o.billing_address_city,
                    o.billing_address_street,
                    o.billing_address_home,
                    o.billing_address_apartment,
                    o.delivery_address_city,
                    o.delivery_address_street,
                    o.delivery_address_home,
                    o.delivery_address_apartment,
                    promocode.discount_value,
                    promocode.type,
                    promocode.valid_to,
                    promocode.code,
                    d.id,
                    d.title,
                    d.description,
                    d.price
                """;

        var command = new CommandDefinition(
            sql,
            new { UserId = userId.Value, id = orderId.Value },
            cancellationToken: ct
        );

        var item = await connection.QueryAsync<
            OrderResponse,
            PromocodeResponse,
            DeliveryOptionResponse,
            OrderResponse
        >(
            command,
            (sku, promocode, deliveryOption) =>
                sku with
                {
                    Promocode = promocode,
                    DeliveryOption = deliveryOption,
                },
            splitOn: "DiscountValue, Id"
        );

        return item.FirstOrDefault();
    }

    public async Task<KeysetPaginated<OrderResponse, Guid>> GetOrdersByUserIdAsync(
        UserId userId,
        KeysetPagination<Guid> keysetPagination,
        CancellationToken ct = default
    )
    {
        throw new NotImplementedException();
    }

    public async Task<bool> IsPromocodeUsedByUserAsync(
        UserId userId,
        PromocodeId promocodeId,
        CancellationToken ct = default
    )
    {
        throw new NotImplementedException();
    }
}
