using Application.Admin.Categories.Constants;
using Application.Admin.Categories.UseCases.GetCategories;
using Application.Auth.Types;
using Domain.Product.Category;
using Presentation.Admin.Categories.Dto;

namespace Presentation.Admin.Categories.Endpoints;

public sealed record GetCategoriesRequest(
    string? Search,
    int? Limit,
    string? PrevCursor,
    string? NextCursor
);

public sealed class GetCategoriesRequestValidator : AbstractValidator<GetCategoriesRequest>
{
    public GetCategoriesRequestValidator()
    {
        RuleFor(x => x.Search).MaximumLength(CategoriesConstants.GetCategoriesSearchMaxLength);

        RuleFor(x => x.Limit).InclusiveBetween(1, CategoriesConstants.GetCategoriesMaxLimit);

        RuleFor(x => x)
            .Must(x => !(x.PrevCursor is not null && x.NextCursor is not null))
            .WithMessage("Only one cursor can be specified: PrevCursor or NextCursor.")
            .WithName("prevCursor");

        RuleFor(x => x.PrevCursor).MaximumLength(CategoriesConstants.GetCategoriesCursorMaxLength);

        RuleFor(x => x.NextCursor).MaximumLength(CategoriesConstants.GetCategoriesCursorMaxLength);
    }
}

internal static partial class AdminCategoriesEndpoints
{
    private static IEndpointRouteBuilder GetCategoriesV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet(
                "/",
                async (
                    HttpContext ctx,
                    IQueryHandler<
                        GetCategoriesQuery,
                        KeysetPaginated<Category, CategoryId>
                    > queryHandler,
                    [AsParameters] GetCategoriesRequest request,
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

                    var logger = loggerFactory.CreateLogger("Admin.GetCategories");

                    var queryResult = request.ToQuery();
                    if (queryResult.IsFailure)
                    {
                        return ErrorHandler.Handle(queryResult.Error, logger);
                    }

                    var result = await queryHandler.Handle(queryResult.Value, ct);
                    if (result.IsFailure)
                    {
                        return ErrorHandler.Handle(result.Error, logger);
                    }

                    return Results.Ok(
                        new ApiCursorResponse<CategoryDto>(
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
            .Produces<ApiCursorResponse<CategoryDto>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("GetCategories")
            .WithSummary("Retrieves a paginated list of categories for admin panel.")
            .WithDescription(
                "Returns a filtered and paginated list of categories. Supports search and keyset pagination for efficient navigation through large datasets."
            );

        return endpoint;
    }

    private static Result<GetCategoriesQuery> ToQuery(this GetCategoriesRequest request)
    {
        var limit = PositiveInt
            .Create(request.Limit ?? CategoriesConstants.GetCategoriesDefaultLimit)
            .Value;

        NonEmptyString? search = null;
        KeysetCursor<CategoryId>? prev = null;
        KeysetCursor<CategoryId>? next = null;

        if (request.Search is not null)
        {
            var searchResult = NonEmptyString.Create(request.Search);
            if (searchResult.IsFailure)
            {
                return Result<GetCategoriesQuery>.Failure(searchResult.Error);
            }

            search = searchResult.Value;
        }

        if (request.PrevCursor is not null)
        {
            var prevCursorResult = KeysetCursor<CategoryId>.Create(
                request.PrevCursor,
                s =>
                    !Guid.TryParse(s, out var id)
                        ? Result<CategoryId>.Failure(Error.Failure("Invalid category id"))
                        : Result<CategoryId>.Success(new CategoryId(id))
            );

            if (prevCursorResult.IsFailure)
            {
                return Result<GetCategoriesQuery>.Failure(prevCursorResult.Error);
            }

            prev = prevCursorResult.Value;
        }

        if (request.NextCursor is not null)
        {
            var nextCursorResult = KeysetCursor<CategoryId>.Create(
                request.NextCursor,
                s =>
                    !Guid.TryParse(s, out var id)
                        ? Result<CategoryId>.Failure(Error.Failure("Invalid category id"))
                        : Result<CategoryId>.Success(new CategoryId(id))
            );

            if (nextCursorResult.IsFailure)
            {
                return Result<GetCategoriesQuery>.Failure(nextCursorResult.Error);
            }

            next = nextCursorResult.Value;
        }

        var pagination = new KeysetPagination<CategoryId>(limit, prev, next);

        return Result<GetCategoriesQuery>.Success(new GetCategoriesQuery(search, pagination));
    }
}
