using Application.Admin.Products.ProductSkus.Constants;
using Application.ProductSkus.UseCases.GetProductSkus;
using Domain.Brands;
using Domain.Categories;
using Domain.Products;
using Domain.Products.ProductSkus;
using Domain.Shared.ValueObjects;
using Presentation.ProductSkus.Dto;
using Presentation.Shared.Extensions;

namespace Presentation.ProductSkus.Endpoints;

public sealed record GetProductSkusRequest(
    [FromQuery] int[]? Sizes,
    [FromQuery] string[]? Colors,
    [FromQuery] string[]? CategoryIds,
    [FromQuery] string[]? BrandIds,
    [FromQuery] string[]? Genders,
    decimal? MinPrice,
    decimal? MaxPrice,
    int? Limit,
    string? PrevCursor,
    string? NextCursor
);

public sealed class GetProductSkusRequestValidator : AbstractValidator<GetProductSkusRequest>
{
    public GetProductSkusRequestValidator()
    {
        RuleFor(x => x.MinPrice)
            .GreaterThan(0)
            .ValidateNullableValueObject(x => Money.FromDollars(x!.Value))
            .When(x => x.MinPrice is not null);
        RuleFor(x => x.MaxPrice)
            .GreaterThan(0)
            .ValidateNullableValueObject(x => Money.FromDollars(x!.Value))
            .When(x => x.MaxPrice is not null);

        RuleFor(x => x.Sizes).ValidateEachValueObject(x => PositiveInt.Create(x, label: "Size"));
        RuleFor(x => x.Colors)
            .ValidateEachValueObject(x => ProductSkuColor.Create(Uri.UnescapeDataString(x)));

        RuleFor(x => x.CategoryIds)
            .Must(ids => ids is null || ids.All(id => Guid.TryParse(id, out _)))
            .WithMessage("Invalid category id");

        RuleFor(x => x.BrandIds)
            .Must(ids => ids is null || ids.All(id => Guid.TryParse(id, out _)))
            .WithMessage("Invalid brand id");

        RuleFor(x => x.Genders)
            .Must(genders =>
                genders is null
                || genders.All(gender => Enum.TryParse<ProductGender>(gender, true, out _))
            )
            .WithMessage(
                $"Gender must be one of: {string.Join(", ", Enum.GetNames<ProductGender>())}"
            );

        RuleFor(x => x)
            .Must(x => x.MinPrice is null || x.MaxPrice is null || x.MinPrice <= x.MaxPrice)
            .WithMessage("Min price must be less than or equal to max price.")
            .WithName("minPrice");

        RuleFor(x => x)
            .Must(x => x.PrevCursor is null || x.NextCursor is null)
            .WithMessage("Only one cursor can be specified: PrevCursor or NextCursor.")
            .WithName("prevCursor");

        RuleFor(x => x.Limit).InclusiveBetween(1, ProductSkusConstants.GetProductSkusMaxLimit);

        RuleFor(x => x.PrevCursor)
            .ValidateNullableValueObject(x =>
                KeysetCursor<ProductSkuId>.Create(
                    x,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid product id")
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
                            ? Error.Failure("Invalid product id")
                            : new ProductSkuId(id)
                )
            )
            .MaximumLength(ProductSkusConstants.GetProductSkusCursorMaxLength);
    }
}

internal static partial class CartEndpoints
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

                    var query = request.ToQuery();
                    var result = await queryHandler.Handle(query, ct);

                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
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
            .WithDescription("Returns a filtered and paginated list of product skus.")
            .RequireRateLimiting(RateLimitConstants.GetProductSkus);

        return endpoint;
    }

    private static GetProductSkusQuery ToQuery(this GetProductSkusRequest request)
    {
        var limit = PositiveInt
            .Create(request.Limit ?? ProductSkusConstants.GetProductSkusDefaultLimit)
            .Value;

        var sizes = request.Sizes is { Length: > 0 }
            ? request.Sizes.Select(size => PositiveInt.Create(size).Value).ToList()
            : null;

        var colors = request.Colors is { Length: > 0 }
            ? request.Colors.Select(x => ProductSkuColor.Create(x).Value).ToList()
            : null;

        var categoryIds = request.CategoryIds is { Length: > 0 }
            ? request.CategoryIds.Select(id => new CategoryId(Guid.Parse(id))).ToList()
            : null;

        var brandIds = request.BrandIds is { Length: > 0 }
            ? request.BrandIds.Select(id => new BrandId(Guid.Parse(id))).ToList()
            : null;

        var genders = request.Genders is { Length: > 0 }
            ? request.Genders.Select(gender => Enum.Parse<ProductGender>(gender, true)).ToList()
            : null;

        var minPrice = request.MinPrice is not null
            ? Money.FromDollars(request.MinPrice.Value).Value
            : null;
        var maxPrice = request.MaxPrice is not null
            ? Money.FromDollars(request.MaxPrice.Value).Value
            : null;

        var prev = request.PrevCursor is not null
            ? KeysetCursor<ProductSkuId>
                .Create(
                    request.PrevCursor,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid product id")
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
                            ? Error.Failure("Invalid product id")
                            : new ProductSkuId(id)
                )
                .Value
            : null;

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

        return new GetProductSkusQuery(filters, pagination);
    }
}
