using Application.Admin.Products.ProductSkus.Constants;
using Application.Admin.Products.ProductSkus.UseCases.GetProductSkus;
using Application.Auth.Types;
using Domain.Product.ProductSku;
using Presentation.Admin.Products.ProductSkus.Dto;
using Presentation.Shared;

namespace Presentation.Admin.Products.ProductSkus.Endpoints;

public sealed record GetProductSkusRequest(
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

public sealed class GetProductSkusRequestValidator : AbstractValidator<GetProductSkusRequest>
{
    public GetProductSkusRequestValidator()
    {
        RuleFor(x => x.Limit).InclusiveBetween(1, ProductSkusConstants.GetProductSkusMaxLimit);

        RuleFor(x => x.MinPrice).GreaterThan(0);
        RuleFor(x => x.MaxPrice).GreaterThan(0);
        RuleFor(x => x.MinSalePrice).GreaterThan(0);
        RuleFor(x => x.MaxSalePrice).GreaterThan(0);

        RuleFor(x => x)
            .Must(x => x.MinPrice is null || x.MaxPrice is null || x.MinPrice <= x.MaxPrice)
            .WithMessage("MinPrice must be less than or equal to MaxPrice.")
            .WithName("minPrice");

        RuleFor(x => x)
            .Must(x =>
                x.MinSalePrice is null || x.MaxSalePrice is null || x.MinSalePrice <= x.MaxSalePrice
            )
            .WithMessage("MinSalePrice must be less than or equal to MaxSalePrice.")
            .WithName("minSalePrice");

        RuleFor(x => x.Sku).MaximumLength(ProductSkuSku.MaxLength);

        RuleFor(x => x.Size).GreaterThan(0);

        RuleFor(x => x)
            .Must(x => x.PrevCursor is null || x.NextCursor is null)
            .WithMessage("Only one cursor can be specified: PrevCursor or NextCursor.")
            .WithName("prevCursor");

        RuleFor(x => x.PrevCursor)
            .MaximumLength(ProductSkusConstants.GetProductSkusCursorMaxLength);

        RuleFor(x => x.NextCursor)
            .MaximumLength(ProductSkusConstants.GetProductSkusCursorMaxLength);
    }
}

internal static partial class AdminProductSkusEndpoints
{
    private static IEndpointRouteBuilder GetProductSkusV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet(
                "/skus",
                async (
                    HttpContext ctx,
                    [AsParameters] GetProductSkusRequest request,
                    IQueryHandler<
                        GetProductSkusQuery,
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

                    var logger = loggerFactory.CreateLogger("Admin.GetProductSkus");

                    var queryResult = request.ToQuery();
                    if (queryResult.IsFailure)
                    {
                        return ErrorHandler.Handle(queryResult.Error, logger);
                    }

                    var result = await queryHandler.Handle(queryResult.Value, ct);
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
            .WithName("GetProductSkus")
            .WithSummary("Retrieves a paginated list of product skus for admin panel.")
            .WithDescription("Returns a filtered and paginated list of product skus.");

        return endpoint;
    }

    private static Result<GetProductSkusQuery> ToQuery(this GetProductSkusRequest request)
    {
        var limit = PositiveInt
            .Create(request.Limit ?? ProductSkusConstants.GetProductSkusDefaultLimit)
            .Value;

        PositiveInt? minPrice = null;
        PositiveInt? maxPrice = null;
        PositiveInt? minSalePrice = null;
        PositiveInt? maxSalePrice = null;
        PositiveInt? size = null;
        ProductSkuColor? color = null;
        ProductSkuSku? sku = null;
        KeysetCursor<ProductSkuId>? prev = null;
        KeysetCursor<ProductSkuId>? next = null;

        if (request.MinPrice is not null)
        {
            var minPriceResult = PositiveInt.Create(request.MinPrice.Value, "minPrice");
            if (minPriceResult.IsFailure)
            {
                return Result<GetProductSkusQuery>.Failure(minPriceResult.Error);
            }

            minPrice = minPriceResult.Value;
        }

        if (request.MaxPrice is not null)
        {
            var maxPriceResult = PositiveInt.Create(request.MaxPrice.Value, "maxPrice");
            if (maxPriceResult.IsFailure)
            {
                return Result<GetProductSkusQuery>.Failure(maxPriceResult.Error);
            }

            maxPrice = maxPriceResult.Value;
        }

        if (request.MinSalePrice is not null)
        {
            var minSalePriceResult = PositiveInt.Create(request.MinSalePrice.Value, "minSalePrice");
            if (minSalePriceResult.IsFailure)
            {
                return Result<GetProductSkusQuery>.Failure(minSalePriceResult.Error);
            }

            minSalePrice = minSalePriceResult.Value;
        }

        if (request.MaxSalePrice is not null)
        {
            var maxSalePriceResult = PositiveInt.Create(request.MaxSalePrice.Value, "maxSalePrice");
            if (maxSalePriceResult.IsFailure)
            {
                return Result<GetProductSkusQuery>.Failure(maxSalePriceResult.Error);
            }

            maxSalePrice = maxSalePriceResult.Value;
        }

        if (request.Size is not null)
        {
            var sizeResult = PositiveInt.Create(request.Size.Value, "size");
            if (sizeResult.IsFailure)
            {
                return Result<GetProductSkusQuery>.Failure(sizeResult.Error);
            }

            size = sizeResult.Value;
        }

        if (request.Color is not null)
        {
            var colorResult = ProductSkuColor.Create(request.Color);
            if (colorResult.IsFailure)
            {
                return Result<GetProductSkusQuery>.Failure(colorResult.Error);
            }

            color = colorResult.Value;
        }

        if (request.Sku is not null)
        {
            var skuResult = ProductSkuSku.Create(request.Sku);
            if (skuResult.IsFailure)
            {
                return Result<GetProductSkusQuery>.Failure(skuResult.Error);
            }

            sku = skuResult.Value;
        }

        if (request.PrevCursor is not null)
        {
            var prevCursorResult = KeysetCursor<ProductSkuId>.Create(
                request.PrevCursor,
                s =>
                    !Guid.TryParse(s, out var id)
                        ? Result<ProductSkuId>.Failure(Error.Failure("Invalid product id"))
                        : Result<ProductSkuId>.Success(new ProductSkuId(id))
            );

            if (prevCursorResult.IsFailure)
            {
                return Result<GetProductSkusQuery>.Failure(prevCursorResult.Error);
            }

            prev = prevCursorResult.Value;
        }

        if (request.NextCursor is not null)
        {
            var nextCursorResult = KeysetCursor<ProductSkuId>.Create(
                request.NextCursor,
                s =>
                    !Guid.TryParse(s, out var id)
                        ? Result<ProductSkuId>.Failure(Error.Failure("Invalid product id"))
                        : Result<ProductSkuId>.Success(new ProductSkuId(id))
            );

            if (nextCursorResult.IsFailure)
            {
                return Result<GetProductSkusQuery>.Failure(nextCursorResult.Error);
            }

            next = nextCursorResult.Value;
        }

        var pagination = new KeysetPagination<ProductSkuId>(limit, prev, next);
        var filters = new ProductSkusFilters(
            request.InStock,
            minPrice,
            maxPrice,
            minSalePrice,
            maxSalePrice,
            size,
            color,
            sku
        );

        return Result<GetProductSkusQuery>.Success(new GetProductSkusQuery(filters, pagination));
    }
}
