using Application.Admin.Products.Constants;
using Application.Admin.Products.UseCases.GetProducts;
using Application.Auth.Types;
using Domain.Product;
using Domain.Product.Brand;
using Domain.Product.Category;
using Presentation.Admin.Products.Dto;

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
        RuleFor(x => x.Search).MaximumLength(ProductsConstants.GetProductsSearchMaxLength);

        RuleFor(x => x.Limit).InclusiveBetween(1, ProductsConstants.GetProductsMaxLimit);

        RuleFor(x => x.Gender)
            .Must(g => g is null || Enum.TryParse<ProductGender>(g, true, out _))
            .WithMessage("'Gender' must be one of Men, Women, Unisex");

        RuleFor(x => x)
            .Must(x => !(x.PrevCursor is not null && x.NextCursor is not null))
            .WithMessage("Only one cursor can be specified: PrevCursor or NextCursor.")
            .WithName("prevCursor");

        RuleFor(x => x.PrevCursor).MaximumLength(ProductsConstants.GetProductsCursorMaxLength);

        RuleFor(x => x.NextCursor).MaximumLength(ProductsConstants.GetProductsCursorMaxLength);
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

                    var commandResult = request.ToCommand();
                    if (commandResult.IsFailure)
                    {
                        return ErrorHandler.Handle(commandResult.Error, logger);
                    }

                    var result = await queryHandler.Handle(commandResult.Value, ct);
                    return result.IsFailure
                        ? ErrorHandler.Handle(result.Error, logger)
                        : Results.Ok(
                            new ApiCursorResponse<ProductDto>(
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
            .Produces<ApiResponse<ProductDto>>()
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

    private static Result<GetProductsQuery> ToCommand(this GetProductsRequest request)
    {
        var limit = PositiveInt
            .Create(request.Limit ?? ProductsConstants.GetProductsDefaultLimit)
            .Value;

        NonEmptyString? search = null;
        ProductGender? productGender = null;
        BrandId? brandId = null;
        CategoryId? categoryId = null;
        KeysetCursor<ProductId>? prev = null;
        KeysetCursor<ProductId>? next = null;

        if (request.Search is not null)
        {
            var searchResult = NonEmptyString.Create(request.Search);
            if (searchResult.IsFailure)
            {
                return Result<GetProductsQuery>.Failure(searchResult.Error);
            }

            search = searchResult.Value;
        }

        if (request.Gender is not null)
        {
            if (!Enum.TryParse<ProductGender>(request.Gender, true, out var gender))
            {
                return Result<GetProductsQuery>.Failure(
                    Error.Validation("gender", ["Invalid gender"])
                );
            }

            productGender = gender;
        }

        if (request.BrandId is not null)
        {
            if (!Guid.TryParse(request.BrandId, out var id))
            {
                return Result<GetProductsQuery>.Failure(
                    Error.Validation("brandId", ["Invalid brand id"])
                );
            }

            brandId = new BrandId(id);
        }

        if (request.CategoryId is not null)
        {
            if (!Guid.TryParse(request.CategoryId, out var id))
            {
                return Result<GetProductsQuery>.Failure(
                    Error.Validation("categoryId", ["Invalid category id"])
                );
            }

            categoryId = new CategoryId(id);
        }

        if (request.PrevCursor is not null)
        {
            var prevCursorResult = KeysetCursor<ProductId>.Create(
                request.PrevCursor,
                s =>
                    !Guid.TryParse(s, out var id)
                        ? Result<ProductId>.Failure(Error.Failure("Invalid product id"))
                        : Result<ProductId>.Success(new ProductId(id))
            );

            if (prevCursorResult.IsFailure)
            {
                return Result<GetProductsQuery>.Failure(prevCursorResult.Error);
            }

            prev = prevCursorResult.Value;
        }

        if (request.NextCursor is not null)
        {
            var nextCursorResult = KeysetCursor<ProductId>.Create(
                request.NextCursor,
                s =>
                    !Guid.TryParse(s, out var id)
                        ? Result<ProductId>.Failure(Error.Failure("Invalid product id"))
                        : Result<ProductId>.Success(new ProductId(id))
            );

            if (nextCursorResult.IsFailure)
            {
                return Result<GetProductsQuery>.Failure(nextCursorResult.Error);
            }

            next = nextCursorResult.Value;
        }

        var pagination = new KeysetPagination<ProductId>(limit, prev, next);
        var filters = new ProductFilters(
            search,
            productGender,
            brandId,
            categoryId,
            request.IsDeleted
        );

        return Result<GetProductsQuery>.Success(new GetProductsQuery(filters, pagination));
    }
}
