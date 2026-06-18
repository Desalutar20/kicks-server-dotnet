using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "brand",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(
                        type: "character varying(30)",
                        maxLength: 30,
                        nullable: false
                    ),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_brand", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(
                        type: "character varying(30)",
                        maxLength: 30,
                        nullable: false
                    ),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "delivery_options",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_delivery_options", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "outbox",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    data = table.Column<string>(type: "jsonb", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(
                        type: "timestamptz",
                        nullable: true
                    ),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox", x => x.id);
                    table.CheckConstraint("CK_type", "type IN ('email', 'file')");
                }
            );

            migrationBuilder.CreateTable(
                name: "promocode",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    discount_value = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    usage_limit = table.Column<int>(type: "integer", nullable: false),
                    usage_count = table.Column<int>(
                        type: "integer",
                        nullable: false,
                        defaultValue: 0
                    ),
                    code = table.Column<string>(type: "text", nullable: false),
                    valid_from = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    valid_to = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_promocode", x => x.id);
                    table.CheckConstraint(
                        "CK_discount_value_percent",
                        "type != 'percent' OR (discount_value > 0 AND discount_value < 100)"
                    );
                    table.CheckConstraint("CK_type", "type IN ('fixed', 'percent')");
                    table.CheckConstraint("CK_usage_count", "usage_count < usage_limit");
                    table.CheckConstraint("CK_validity_period", "valid_from < valid_to");
                }
            );

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(
                        type: "character varying(30)",
                        maxLength: 30,
                        nullable: true
                    ),
                    last_name = table.Column<string>(
                        type: "character varying(30)",
                        maxLength: 30,
                        nullable: true
                    ),
                    email = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    hashed_password = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: true
                    ),
                    gender = table.Column<string>(type: "text", nullable: true),
                    role = table.Column<string>(
                        type: "text",
                        nullable: false,
                        defaultValue: "regular"
                    ),
                    google_id = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: true
                    ),
                    facebook_id = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: true
                    ),
                    is_verified = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                    is_banned = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.CheckConstraint("CK_gender", "gender IN ('male', 'female', 'other')");
                    table.CheckConstraint("CK_role", "role IN ('regular', 'admin')");
                }
            );

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(
                        type: "character varying(60)",
                        maxLength: 60,
                        nullable: false
                    ),
                    description = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    gender = table.Column<string>(type: "text", nullable: false),
                    tags = table.Column<List<string>>(
                        type: "text[]",
                        nullable: false,
                        defaultValueSql: "'{}'::text[]"
                    ),
                    is_deleted = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                    brand_id = table.Column<Guid>(type: "uuid", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product", x => x.id);
                    table.CheckConstraint("CK_gender", "gender IN ('men', 'women', 'unisex')");
                    table.ForeignKey(
                        name: "fk_product_brand_brand_id",
                        column: x => x.brand_id,
                        principalTable: "brand",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull,
                        onUpdate: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_product_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull,
                        onUpdate: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "cart",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    promocode_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cart", x => x.id);
                    table.ForeignKey(
                        name: "fk_cart_promocode_promocode_id",
                        column: x => x.promocode_id,
                        principalTable: "promocode",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull,
                        onUpdate: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_cart_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade,
                        onUpdate: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "order",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(
                        type: "text",
                        nullable: false,
                        defaultValue: "pending"
                    ),
                    delivery_price = table.Column<long>(type: "bigint", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    delivery_option_id = table.Column<Guid>(type: "uuid", nullable: false),
                    promocode_id = table.Column<Guid>(type: "uuid", nullable: true),
                    billing_address_apartment = table.Column<string>(type: "text", nullable: true),
                    billing_address_city = table.Column<string>(type: "text", nullable: true),
                    billing_address_home = table.Column<string>(type: "text", nullable: true),
                    billing_address_street = table.Column<string>(type: "text", nullable: true),
                    delivery_address_apartment = table.Column<string>(
                        type: "text",
                        nullable: false
                    ),
                    delivery_address_city = table.Column<string>(type: "text", nullable: false),
                    delivery_address_home = table.Column<string>(type: "text", nullable: false),
                    delivery_address_street = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order", x => x.id);
                    table.CheckConstraint(
                        "CK_order_status",
                        "status IN ('pending', 'paid', 'shipped', 'delivered', 'cancelled')"
                    );
                    table.ForeignKey(
                        name: "fk_order_delivery_options_delivery_option_id",
                        column: x => x.delivery_option_id,
                        principalTable: "delivery_options",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict,
                        onUpdate: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_order_promocode_promocode_id",
                        column: x => x.promocode_id,
                        principalTable: "promocode",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull,
                        onUpdate: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_order_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade,
                        onUpdate: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "product_sku",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    size = table.Column<int>(type: "integer", nullable: false),
                    color = table.Column<string>(type: "text", nullable: false),
                    sku = table.Column<string>(type: "text", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    price = table.Column<long>(type: "bigint", nullable: false),
                    sale_price = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    images = table.Column<string>(type: "jsonb", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_sku", x => x.id);
                    table.CheckConstraint("CK_product_sku_quantity_positive", "quantity >= 0");
                    table.ForeignKey(
                        name: "fk_product_sku_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade,
                        onUpdate: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "order_payment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    transaction_id = table.Column<string>(
                        type: "character varying(60)",
                        maxLength: 60,
                        nullable: false
                    ),
                    amount = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(
                        type: "text",
                        nullable: false,
                        defaultValue: "pending"
                    ),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_payment", x => x.id);
                    table.CheckConstraint(
                        "CK_order_payment_status",
                        "status IN ('pending', 'completed', 'failed', 'expired', 'refunded', 'cancelled')"
                    );
                    table.ForeignKey(
                        name: "fk_order_payment_order_order_id",
                        column: x => x.order_id,
                        principalTable: "order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade,
                        onUpdate: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "cart_item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    product_sku_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cart_id = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cart_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_cart_item_cart_cart_id",
                        column: x => x.cart_id,
                        principalTable: "cart",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade,
                        onUpdate: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_cart_item_product_sku_product_sku_id",
                        column: x => x.product_sku_id,
                        principalTable: "product_sku",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade,
                        onUpdate: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "order_item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<long>(type: "bigint", nullable: false),
                    product_sku_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_item_order_order_id",
                        column: x => x.order_id,
                        principalTable: "order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade,
                        onUpdate: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_order_item_product_sku_product_sku_id",
                        column: x => x.product_sku_id,
                        principalTable: "product_sku",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict,
                        onUpdate: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "product_sku_review",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(
                        type: "text",
                        nullable: false,
                        defaultValue: "pending"
                    ),
                    product_sku_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    images = table.Column<string>(type: "jsonb", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_sku_review", x => x.id);
                    table.CheckConstraint(
                        "CK_status",
                        "status IN ('pending', 'rejected', 'approved')"
                    );
                    table.ForeignKey(
                        name: "fk_product_sku_review_product_sku_product_sku_id",
                        column: x => x.product_sku_id,
                        principalTable: "product_sku",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade,
                        onUpdate: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_product_sku_review_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade,
                        onUpdate: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "uq_brand_name",
                table: "brand",
                column: "name",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_cart_promocode_id",
                table: "cart",
                column: "promocode_id"
            );

            migrationBuilder.CreateIndex(
                name: "uq_cart_user",
                table: "cart",
                column: "user_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_cart_item_product_sku_id",
                table: "cart_item",
                column: "product_sku_id"
            );

            migrationBuilder.CreateIndex(
                name: "uq_cart_items_cart_id_product_sku_id",
                table: "cart_item",
                columns: new[] { "cart_id", "product_sku_id" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "uq_category_name",
                table: "category",
                column: "name",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_order_delivery_option_id",
                table: "order",
                column: "delivery_option_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_order_promocode_id",
                table: "order",
                column: "promocode_id"
            );

            migrationBuilder.CreateIndex(
                name: "uq_order_user_pending_unique",
                table: "order",
                column: "user_id",
                unique: true,
                filter: "status = 'pending'"
            );

            migrationBuilder.CreateIndex(
                name: "uq_order_user_promocode",
                table: "order",
                columns: new[] { "user_id", "promocode_id" },
                unique: true,
                filter: "    \"promocode_id\" IS NOT NULL\n    AND \"status\" <> 'cancelled'"
            );

            migrationBuilder.CreateIndex(
                name: "ix_order_item_product_sku_id",
                table: "order_item",
                column: "product_sku_id"
            );

            migrationBuilder.CreateIndex(
                name: "uq_order_items_order_id_product_sku_id",
                table: "order_item",
                columns: new[] { "order_id", "product_sku_id" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_order_payment_order_id",
                table: "order_payment",
                column: "order_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_order_payment_transaction_id",
                table: "order_payment",
                column: "transaction_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "idx_outbox_processed_at_null",
                table: "outbox",
                column: "processed_at",
                filter: "processed_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ix_product_brand_id",
                table: "product",
                column: "brand_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_product_category_id",
                table: "product",
                column: "category_id"
            );

            migrationBuilder.CreateIndex(
                name: "uq_product_title_gender_category_brand",
                table: "product",
                columns: new[] { "title", "gender", "category_id", "brand_id" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "uq_product_sku_product_size_color",
                table: "product_sku",
                columns: new[] { "product_id", "size", "color" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "uq_product_sku_sku",
                table: "product_sku",
                column: "sku",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_product_sku_review_user_id",
                table: "product_sku_review",
                column: "user_id"
            );

            migrationBuilder.CreateIndex(
                name: "uq_product_sku_review_product_sku_user",
                table: "product_sku_review",
                columns: new[] { "product_sku_id", "user_id" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "uq_promocode_code",
                table: "promocode",
                column: "code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_users_facebook_id",
                table: "users",
                column: "facebook_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_users_google_id",
                table: "users",
                column: "google_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "uq_user_email",
                table: "users",
                column: "email",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "cart_item");

            migrationBuilder.DropTable(name: "order_item");

            migrationBuilder.DropTable(name: "order_payment");

            migrationBuilder.DropTable(name: "outbox");

            migrationBuilder.DropTable(name: "product_sku_review");

            migrationBuilder.DropTable(name: "cart");

            migrationBuilder.DropTable(name: "order");

            migrationBuilder.DropTable(name: "product_sku");

            migrationBuilder.DropTable(name: "delivery_options");

            migrationBuilder.DropTable(name: "promocode");

            migrationBuilder.DropTable(name: "users");

            migrationBuilder.DropTable(name: "product");

            migrationBuilder.DropTable(name: "brand");

            migrationBuilder.DropTable(name: "category");
        }
    }
}
