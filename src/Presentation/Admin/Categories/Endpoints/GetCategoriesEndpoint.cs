using Application.Admin.Categories.Constants;
using Application.Admin.Categories.UseCases.GetCategories;
using Application.Auth.Types;
using Domain.Categories;
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
        RuleFor(x => x.Search)
            .ValidateNullableValueObject(x => NonEmptyString.Create(x, label: "Search"))
            .MaximumLength(CategoriesConstants.GetCategoriesSearchMaxLength);

        RuleFor(x => x.Limit).InclusiveBetween(1, CategoriesConstants.GetCategoriesMaxLimit);

        RuleFor(x => x)
            .Must(x => x.PrevCursor is null || x.NextCursor is null)
            .WithMessage("Only one cursor can be specified: PrevCursor or NextCursor.")
            .WithName("prevCursor");

        RuleFor(x => x.PrevCursor)
            .ValidateNullableValueObject(x =>
                KeysetCursor<CategoryId>.Create(
                    x,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid category id")
                            : new CategoryId(id)
                )
            )
            .MaximumLength(CategoriesConstants.GetCategoriesCursorMaxLength);

        RuleFor(x => x.NextCursor)
            .ValidateNullableValueObject(x =>
                KeysetCursor<CategoryId>.Create(
                    x,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid category id")
                            : new CategoryId(id)
                )
            )
            .MaximumLength(CategoriesConstants.GetCategoriesCursorMaxLength);
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

                    var query = request.ToQuery();
                    var result = await queryHandler.Handle(query, ct);

                    if (result.IsFailure)
                    {
                        return ErrorHandler.Handle(result.Error, logger);
                    }

                    return Results.Ok(
                        new ApiCursorResponse<AdminCategoryDto>(
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
            .Produces<ApiCursorResponse<AdminCategoryDto>>()
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

    private static GetCategoriesQuery ToQuery(this GetCategoriesRequest request)
    {
        var limit = PositiveInt
            .Create(request.Limit ?? CategoriesConstants.GetCategoriesDefaultLimit)
            .Value;

        NonEmptyString? search = request.Search is not null
            ? NonEmptyString.Create(request.Search).Value
            : null;
        var prev = request.PrevCursor is not null
            ? KeysetCursor<CategoryId>
                .Create(
                    request.PrevCursor,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid category id")
                            : new CategoryId(id)
                )
                .Value
            : null;
        var next = request.NextCursor is not null
            ? KeysetCursor<CategoryId>
                .Create(
                    request.NextCursor,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid category id")
                            : new CategoryId(id)
                )
                .Value
            : null;

        var pagination = new KeysetPagination<CategoryId>(limit, prev, next);

        return new GetCategoriesQuery(search, pagination);
    }
}
