using Application.Admin.Brands.Constants;
using Application.Admin.Brands.Types;
using Application.Admin.Brands.UseCases.GetBrands;
using Application.Auth.Types;
using Domain.Shared.ValueObjects;
using Presentation.Shared.Extensions;

namespace Presentation.Admin.Brands.Endpoints;

public sealed record GetBrandsRequest(
    string? Search,
    int? Limit,
    string? PrevCursor,
    string? NextCursor
);

public sealed class GetBrandsRequestValidator : AbstractValidator<GetBrandsRequest>
{
    public GetBrandsRequestValidator()
    {
        RuleFor(x => x.Search)
            .ValidateNullableValueObject(x => NonEmptyString.Create(x, label: "Search"))
            .MaximumLength(BrandsConstants.GetBrandsSearchMaxLength);

        RuleFor(x => x.Limit).InclusiveBetween(1, BrandsConstants.GetBrandsMaxLimit);

        RuleFor(x => x)
            .Must(x => x.PrevCursor is null || x.NextCursor is null)
            .WithMessage("Only one cursor can be specified: PrevCursor or NextCursor.")
            .WithName("prevCursor");

        RuleFor(x => x.PrevCursor)
            .ValidateNullableValueObject(x =>
                KeysetCursor<Guid>.Create(
                    x,
                    s => !Guid.TryParse(s, out var id) ? Error.Failure("Invalid brand id") : id
                )
            )
            .MaximumLength(BrandsConstants.GetBrandsCursorMaxLength);

        RuleFor(x => x.NextCursor)
            .ValidateNullableValueObject(x =>
                KeysetCursor<Guid>.Create(
                    x,
                    s => !Guid.TryParse(s, out var id) ? Error.Failure("Invalid brand id") : id
                )
            )
            .MaximumLength(BrandsConstants.GetBrandsCursorMaxLength);
    }
}

internal static partial class AdminBrandsEndpoints
{
    private static IEndpointRouteBuilder GetBrandsV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet(
                "/",
                async (
                    HttpContext ctx,
                    IQueryHandler<
                        GetBrandsQuery,
                        KeysetPaginated<AdminBrandResponse, Guid>
                    > queryHandler,
                    [AsParameters] GetBrandsRequest request,
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

                    var logger = loggerFactory.CreateLogger("Admin.GetBrands");

                    var query = request.ToQuery();
                    var result = await queryHandler.Handle(query, ct);

                    if (result.IsFailure)
                    {
                        return result.Error.ToApiError(logger);
                    }

                    return Results.Ok(
                        new ApiCursorResponse<AdminBrandResponse>(
                            [.. result.Value.Data],
                            result.Value.PrevCursor?.ToString(),
                            result.Value.NextCursor?.ToString()
                        )
                    );
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter(new AuthorizeFilter(Role.Admin))
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiCursorResponse<AdminBrandResponse>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("GetBrands")
            .WithSummary("Retrieves a paginated list of brands for admin panel.")
            .WithDescription(
                "Returns a filtered and paginated list of brands. Supports search and keyset pagination for efficient navigation through large datasets."
            );

        return endpoint;
    }

    private static GetBrandsQuery ToQuery(this GetBrandsRequest request)
    {
        var limit = PositiveInt
            .Create(request.Limit ?? BrandsConstants.GetBrandsDefaultLimit)
            .Value;

        var search = request.Search is not null
            ? NonEmptyString.Create(request.Search).Value
            : null;

        var prev = request.PrevCursor is not null
            ? KeysetCursor<Guid>
                .Create(
                    request.PrevCursor,
                    s => !Guid.TryParse(s, out var id) ? Error.Failure("Invalid brand id") : id
                )
                .Value
            : null;

        var next = request.NextCursor is not null
            ? KeysetCursor<Guid>
                .Create(
                    request.NextCursor,
                    s => !Guid.TryParse(s, out var id) ? Error.Failure("Invalid brand id") : id
                )
                .Value
            : null;

        var pagination = new KeysetPagination<Guid>(limit, prev, next);

        return new GetBrandsQuery(search, pagination);
    }
}
