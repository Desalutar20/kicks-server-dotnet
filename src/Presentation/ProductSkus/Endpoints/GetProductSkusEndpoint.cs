using Application.Admin.Products.ProductSkus.Constants;
using Application.ProductSkus.UseCases.GetProductSkus;
using Domain.Brand;
using Domain.Category;
using Domain.Product;
using Domain.Product.ProductSku;
using Presentation.ProductSkus.Dto;
using Presentation.Shared;

namespace Presentation.ProductSkus.Endpoints;

public sealed record GetProductSkusRequest(
    [FromQuery] int[]? Sizes,
    [FromQuery] string[]? Colors,
    [FromQuery] string[]? CategoryIds,
    [FromQuery] string[]? BrandIds,
    [FromQuery] string[]? Genders,
    int? MinPrice,
    int? MaxPrice,
    int? Limit,
    string? PrevCursor,
    string? NextCursor
);

public sealed class GetProductSkusRequestValidator : AbstractValidator<GetProductSkusRequest>
{
    public GetProductSkusRequestValidator()
    {
        RuleFor(x => x.MinPrice).GreaterThan(0);
        RuleFor(x => x.MaxPrice).GreaterThan(0);

        RuleFor(x => x)
            .Must(x => x.MinPrice is null || x.MaxPrice is null || x.MinPrice <= x.MaxPrice)
            .WithMessage("MinPrice must be less than or equal to MaxPrice.")
            .WithName("minPrice");

        RuleFor(x => x)
            .Must(x => x.PrevCursor is null || x.NextCursor is null)
            .WithMessage("Only one cursor can be specified: PrevCursor or NextCursor.")
            .WithName("prevCursor");

        RuleFor(x => x.Limit).InclusiveBetween(1, ProductSkusConstants.GetProductSkusMaxLimit);

        RuleFor(x => x.PrevCursor)
            .MaximumLength(ProductSkusConstants.GetProductSkusCursorMaxLength);

        RuleFor(x => x.NextCursor)
            .MaximumLength(ProductSkusConstants.GetProductSkusCursorMaxLength);
    }
}

internal static partial class ProductSkusEndpoints
{
    private static IEndpointRouteBuilder GetProductSkusV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet(
                "/",
                async (
                    [AsParameters] GetProductSkusRequest request,
                    IQueryHandler<
                        GetProductSkusQuery,
                        KeysetPaginated<ProductSku, ProductSkuId>
                    > queryHandler,
                    ILoggerFactory loggerFactory,
                    CancellationToken ct
                ) =>
                {
                    var logger = loggerFactory.CreateLogger("GetProductSkus");

                    var queryResult = request.ToQuery();
                    if (queryResult.IsFailure)
                    {
                        return ErrorHandler.Handle(queryResult.Error, logger);
                    }

                    var result = await queryHandler.Handle(queryResult.Value, ct);
                    return result.IsFailure
                        ? ErrorHandler.Handle(result.Error, logger)
                        : Results.Ok(
                            new ApiCursorResponse<ProductSkuDto>(
                                [.. result.Value.Data.Select(u => u.ToDto())],
                                result.Value.PrevCursor?.ToString(),
                                result.Value.NextCursor?.ToString()
                            )
                        );
                }
            )
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiCursorResponse<ProductSkuDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("GetProductSkus")
            .WithSummary("Retrieves a paginated list of product skus.")
            .WithDescription("Returns a filtered and paginated list of product skus.");

        return endpoint;
    }

    private static Result<GetProductSkusQuery> ToQuery(this GetProductSkusRequest request)
    {
        var limit = PositiveInt
            .Create(request.Limit ?? ProductSkusConstants.GetProductSkusDefaultLimit)
            .Value;

        List<PositiveInt>? sizes = null;
        List<ProductSkuColor>? colors = null;
        List<CategoryId>? categoryIds = null;
        List<BrandId>? brandIds = null;
        List<ProductGender>? genders = null;
        PositiveInt? minPrice = null;
        PositiveInt? maxPrice = null;
        KeysetCursor<ProductSkuId>? prev = null;
        KeysetCursor<ProductSkuId>? next = null;

        if (request.Sizes is not null && request.Sizes.Length > 0)
        {
            sizes = [];
            foreach (
                var sizeResult in request.Sizes.Select(size => PositiveInt.Create(size, "sizes"))
            )
            {
                if (sizeResult.IsFailure)
                {
                    return Result<GetProductSkusQuery>.Failure(sizeResult.Error);
                }

                sizes.Add(sizeResult.Value);
            }
        }

        if (request.Colors is not null && request.Colors.Length > 0)
        {
            colors = [];
            foreach (
                var colorResult in request.Colors.Select(color =>
                    ProductSkuColor.Create(Uri.UnescapeDataString(color))
                )
            )
            {
                if (colorResult.IsFailure)
                {
                    return Result<GetProductSkusQuery>.Failure(
                        Error.Validation("colors", colorResult.Error.Errors!.Value.Item2)
                    );
                }

                colors.Add(colorResult.Value);
            }
        }

        if (request.CategoryIds is not null && request.CategoryIds.Length > 0)
        {
            categoryIds = [];

            foreach (var categoryId in request.CategoryIds)
            {
                if (!Guid.TryParse(categoryId, out var id))
                {
                    return Result<GetProductSkusQuery>.Failure(
                        Error.Validation("categoryIds", ["invalid category id"])
                    );
                }

                categoryIds.Add(new CategoryId(id));
            }
        }

        if (request.BrandIds is not null && request.BrandIds.Length > 0)
        {
            brandIds = [];

            foreach (var brandId in request.BrandIds)
            {
                if (!Guid.TryParse(brandId, out var id))
                {
                    return Result<GetProductSkusQuery>.Failure(
                        Error.Validation("brandIds", ["invalid brand id"])
                    );
                }

                brandIds.Add(new BrandId(id));
            }
        }

        if (request.Genders is not null && request.Genders.Length > 0)
        {
            genders = [];

            foreach (var gender in request.Genders)
            {
                if (!Enum.TryParse<ProductGender>(gender, true, out var g))
                {
                    return Result<GetProductSkusQuery>.Failure(
                        Error.Validation("genders", ["Invalid gender"])
                    );
                }

                genders.Add(g);
            }
        }

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
            sizes,
            colors,
            categoryIds,
            brandIds,
            genders,
            minPrice,
            maxPrice
        );

        return Result<GetProductSkusQuery>.Success(new GetProductSkusQuery(filters, pagination));
    }
}
