using Application.Admin.Products.ProductSkus.Constants;
using Application.Admin.Products.ProductSkus.UseCases.GetAdminProductSkus;
using Application.Auth.Types;
using Domain.Products.ProductSkus;
using Presentation.Admin.Products.ProductSkus.Dto;

namespace Presentation.Admin.Products.ProductSkus.Endpoints;

public sealed record GetAdminProductSkusRequest(
    bool? InStock,
    int? MinPrice,
    int? MaxPrice,
    int? MinSalePrice,
    int? MaxSalePrice,
    int? Size,
    string? Color,
    string? Sku,
    int? Limit,
    string? PrevCursor,
    string? NextCursor
);

public sealed class GetProductSkusRequestValidator : AbstractValidator<GetAdminProductSkusRequest>
{
    public GetProductSkusRequestValidator()
    {
        RuleFor(x => x.Limit).InclusiveBetween(1, ProductSkusConstants.GetAdminProductSkusMaxLimit);

        RuleFor(x => x.MinPrice)
            .ValidateNullableValueObject(x => PositiveInt.Create(x!.Value, label: "Min price"));
        RuleFor(x => x.MaxPrice)
            .ValidateNullableValueObject(x => PositiveInt.Create(x!.Value, label: "Max price"));
        RuleFor(x => x.MinSalePrice)
            .ValidateNullableValueObject(x =>
                PositiveInt.Create(x!.Value, label: "Min sale price")
            );
        RuleFor(x => x.MaxSalePrice)
            .ValidateNullableValueObject(x =>
                PositiveInt.Create(x!.Value, label: "Max sale price")
            );

        RuleFor(x => x)
            .Must(x => x.MinPrice is null || x.MaxPrice is null || x.MinPrice <= x.MaxPrice)
            .WithMessage("Min price must be less than or equal to max price.")
            .WithName("minPrice");

        RuleFor(x => x)
            .Must(x =>
                x.MinSalePrice is null || x.MaxSalePrice is null || x.MinSalePrice <= x.MaxSalePrice
            )
            .WithMessage("Min sale price must be less than or equal to max sale price.")
            .WithName("minSalePrice");

        RuleFor(x => x.Sku).ValidateNullableValueObject(ProductSkuSku.Create);
        RuleFor(x => x.Color).ValidateNullableValueObject(ProductSkuColor.Create);

        RuleFor(x => x.Size)
            .ValidateNullableValueObject(x => PositiveInt.Create(x!.Value, label: "Size"));

        RuleFor(x => x)
            .Must(x => x.PrevCursor is null || x.NextCursor is null)
            .WithMessage("Only one cursor can be specified: PrevCursor or NextCursor.")
            .WithName("prevCursor");

        RuleFor(x => x.PrevCursor)
            .ValidateNullableValueObject(x =>
                KeysetCursor<ProductSkuId>.Create(
                    x,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid product sku id")
                            : new ProductSkuId(id)
                )
            )
            .MaximumLength(ProductSkusConstants.GetProductSkusCursorMaxLength);

        RuleFor(x => x.NextCursor)
            .ValidateNullableValueObject(x =>
                KeysetCursor<ProductSkuId>.Create(
                    x,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid product sku id")
                            : new ProductSkuId(id)
                )
            )
            .MaximumLength(ProductSkusConstants.GetProductSkusCursorMaxLength);
    }
}

internal static partial class AdminProductSkusEndpoints
{
    private static IEndpointRouteBuilder GetAdminProductSkusV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet(
                "/skus",
                async (
                    HttpContext ctx,
                    [AsParameters] GetAdminProductSkusRequest request,
                    IQueryHandler<
                        GetAdminProductSkusQuery,
                        KeysetPaginated<ProductSku, ProductSkuId>
                    > queryHandler,
                    ILoggerFactory loggerFactory,
                    CancellationToken ct
                ) =>
                {
                    if (
                        !ctx.Items.TryGetValue(RequestConstants.UserKey, out var user)
                        || user is not SessionUser
                    )
                    {
                        return Results.Unauthorized();
                    }

                    var logger = loggerFactory.CreateLogger("Admin.GetAdminProductSkus");

                    var query = request.ToQuery();
                    var result = await queryHandler.Handle(query, ct);

                    return result.IsFailure
                        ? ErrorHandler.Handle(result.Error, logger)
                        : Results.Ok(
                            new ApiCursorResponse<AdminProductSkuDto>(
                                [.. result.Value.Data.Select(u => u.ToDto())],
                                result.Value.PrevCursor?.ToString(),
                                result.Value.NextCursor?.ToString()
                            )
                        );
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter(new AuthorizeFilter(Role.Admin))
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiCursorResponse<AdminProductSkuDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("GetAdminProductSkus")
            .WithSummary("Retrieves a paginated list of admin product skus for admin panel.")
            .WithDescription("Returns a filtered and paginated list of product skus.");

        return endpoint;
    }

    private static GetAdminProductSkusQuery ToQuery(this GetAdminProductSkusRequest request)
    {
        var limit = PositiveInt
            .Create(request.Limit ?? ProductSkusConstants.GetAdminProductSkusDefaultLimit)
            .Value;

        var minPrice = request.MinPrice is not null
            ? PositiveInt.Create(request.MinPrice.Value).Value
            : null;
        var maxPrice = request.MaxPrice is not null
            ? PositiveInt.Create(request.MaxPrice.Value).Value
            : null;
        var minSalePrice = request.MinSalePrice is not null
            ? PositiveInt.Create(request.MinSalePrice.Value).Value
            : null;
        var maxSalePrice = request.MaxSalePrice is not null
            ? PositiveInt.Create(request.MaxSalePrice.Value).Value
            : null;

        var size = request.Size is not null ? PositiveInt.Create(request.Size.Value).Value : null;

        var color = request.Color is not null ? ProductSkuColor.Create(request.Color).Value : null;
        var sku = request.Sku is not null ? ProductSkuSku.Create(request.Sku).Value : null;

        var prev = request.PrevCursor is not null
            ? KeysetCursor<ProductSkuId>
                .Create(
                    request.PrevCursor,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid product sku id")
                            : new ProductSkuId(id)
                )
                .Value
            : null;
        var next = request.NextCursor is not null
            ? KeysetCursor<ProductSkuId>
                .Create(
                    request.NextCursor,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid product sku id")
                            : new ProductSkuId(id)
                )
                .Value
            : null;

        var pagination = new KeysetPagination<ProductSkuId>(limit, prev, next);
        var filters = new AdminProductSkusFilters(
            request.InStock,
            minPrice,
            maxPrice,
            minSalePrice,
            maxSalePrice,
            size,
            color,
            sku
        );

        return new GetAdminProductSkusQuery(filters, pagination);
    }
}
