using Application.Admin.Products.Constants;
using Application.Admin.Products.UseCases.GetProducts;
using Application.Auth.Types;
using Domain.Brands;
using Domain.Categories;
using Domain.Products;
using Domain.Shared.ValueObjects;
using Presentation.Admin.Products.Dto;
using Presentation.Shared.Extensions;

namespace Presentation.Admin.Products.Endpoints;

public sealed record GetProductsRequest(
    string? Search,
    string? Gender,
    string? BrandId,
    string? CategoryId,
    bool? IsDeleted,
    int? Limit,
    string? PrevCursor,
    string? NextCursor
);

public sealed class GetProductsRequestValidator : AbstractValidator<GetProductsRequest>
{
    public GetProductsRequestValidator()
    {
        RuleFor(x => x.Search)
            .ValidateNullableValueObject(x => NonEmptyString.Create(x, label: "Search"))
            .MaximumLength(ProductsConstants.GetProductsSearchMaxLength);

        RuleFor(x => x.Limit).InclusiveBetween(1, ProductsConstants.GetProductsMaxLimit);

        RuleFor(x => x.Gender)
            .Must(g => g is null || Enum.TryParse<ProductGender>(g, true, out _))
            .WithMessage(
                $"Gender must be one of: {string.Join(", ", Enum.GetNames<ProductGender>())}"
            );

        RuleFor(x => x.CategoryId)
            .Must(x => x is null || Guid.TryParse(x, out _))
            .WithMessage("Invalid category id");

        RuleFor(x => x.BrandId)
            .Must(x => x is null || Guid.TryParse(x, out _))
            .WithMessage("Invalid brand id");

        RuleFor(x => x)
            .Must(x => x.PrevCursor is null || x.NextCursor is null)
            .WithMessage("Only one cursor can be specified: PrevCursor or NextCursor.")
            .WithName("prevCursor");

        RuleFor(x => x.PrevCursor)
            .ValidateNullableValueObject(x =>
                KeysetCursor<ProductId>.Create(
                    x,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid product id")
                            : new ProductId(id)
                )
            )
            .MaximumLength(ProductsConstants.GetProductsCursorMaxLength);

        RuleFor(x => x.NextCursor)
            .ValidateNullableValueObject(x =>
                KeysetCursor<ProductId>.Create(
                    x,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid product id")
                            : new ProductId(id)
                )
            )
            .MaximumLength(ProductsConstants.GetProductsCursorMaxLength);
    }
}

internal static partial class AdminProductsEndpoints
{
    private static IEndpointRouteBuilder GetProductsV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet(
                "/",
                async (
                    HttpContext ctx,
                    [AsParameters] GetProductsRequest request,
                    IQueryHandler<
                        GetProductsQuery,
                        KeysetPaginated<Product, ProductId>
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

                    var logger = loggerFactory.CreateLogger("Admin.GetProducts");

                    var query = request.ToQuery();
                    var result = await queryHandler.Handle(query, ct);

                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
                        : Results.Ok(
                            new ApiCursorResponse<AdminProductDto>(
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
            .Produces<ApiResponse<AdminProductDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("GetProducts")
            .WithSummary("Retrieves a paginated list of products for admin panel.")
            .WithDescription(
                "Returns a filtered and paginated list of products. Supports search and keyset pagination for efficient navigation through large datasets."
            );

        return endpoint;
    }

    private static GetProductsQuery ToQuery(this GetProductsRequest request)
    {
        var limit = PositiveInt
            .Create(request.Limit ?? ProductsConstants.GetProductsDefaultLimit)
            .Value;

        var search = request.Search is not null
            ? NonEmptyString.Create(request.Search).Value
            : null;
        ProductGender? productGender = request.Gender is not null
            ? Enum.Parse<ProductGender>(request.Gender, true)
            : null;
        var brandId = request.BrandId is not null ? new BrandId(Guid.Parse(request.BrandId)) : null;
        var categoryId = request.CategoryId is not null
            ? new CategoryId(Guid.Parse(request.CategoryId))
            : null;
        var prev = request.PrevCursor is not null
            ? KeysetCursor<ProductId>
                .Create(
                    request.PrevCursor,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid product id")
                            : new ProductId(id)
                )
                .Value
            : null;
        var next = request.NextCursor is not null
            ? KeysetCursor<ProductId>
                .Create(
                    request.NextCursor,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid product id")
                            : new ProductId(id)
                )
                .Value
            : null;

        var pagination = new KeysetPagination<ProductId>(limit, prev, next);
        var filters = new ProductFilters(
            search,
            productGender,
            brandId,
            categoryId,
            request.IsDeleted
        );

        return new GetProductsQuery(filters, pagination);
    }
}
