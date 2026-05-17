using Application.Admin.Brands.Constants;
using Application.Admin.Brands.UseCases.GetBrands;
using Application.Auth.Types;
using Domain.Brand;
using Presentation.Admin.Brands.Dto;
using Presentation.Shared;

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
        RuleFor(x => x.Search).MaximumLength(BrandsConstants.GetBrandsSearchMaxLength);

        RuleFor(x => x.Limit).InclusiveBetween(1, BrandsConstants.GetBrandsMaxLimit);

        RuleFor(x => x)
            .Must(x => x.PrevCursor is null || x.NextCursor is null)
            .WithMessage("Only one cursor can be specified: PrevCursor or NextCursor.")
            .WithName("prevCursor");

        RuleFor(x => x.PrevCursor).MaximumLength(BrandsConstants.GetBrandsCursorMaxLength);

        RuleFor(x => x.NextCursor).MaximumLength(BrandsConstants.GetBrandsCursorMaxLength);
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
                    IQueryHandler<GetBrandsQuery, KeysetPaginated<Brand, BrandId>> queryHandler,
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
                        new ApiCursorResponse<AdminBrandDto>(
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
            .Produces<ApiCursorResponse<AdminBrandDto>>()
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

    private static Result<GetBrandsQuery> ToQuery(this GetBrandsRequest request)
    {
        var limit = PositiveInt
            .Create(request.Limit ?? BrandsConstants.GetBrandsDefaultLimit)
            .Value;

        NonEmptyString? search = null;
        KeysetCursor<BrandId>? prev = null;
        KeysetCursor<BrandId>? next = null;

        if (request.Search is not null)
        {
            var searchResult = NonEmptyString.Create(request.Search);
            if (searchResult.IsFailure)
            {
                return Result<GetBrandsQuery>.Failure(searchResult.Error);
            }

            search = searchResult.Value;
        }

        if (request.PrevCursor is not null)
        {
            var prevCursorResult = KeysetCursor<BrandId>.Create(
                request.PrevCursor,
                s =>
                    !Guid.TryParse(s, out var id)
                        ? Result<BrandId>.Failure(Error.Failure("Invalid brand id"))
                        : Result<BrandId>.Success(new BrandId(id))
            );

            if (prevCursorResult.IsFailure)
            {
                return Result<GetBrandsQuery>.Failure(prevCursorResult.Error);
            }

            prev = prevCursorResult.Value;
        }

        if (request.NextCursor is not null)
        {
            var nextCursorResult = KeysetCursor<BrandId>.Create(
                request.NextCursor,
                s =>
                    !Guid.TryParse(s, out var id)
                        ? Result<BrandId>.Failure(Error.Failure("Invalid brand id"))
                        : Result<BrandId>.Success(new BrandId(id))
            );

            if (nextCursorResult.IsFailure)
            {
                return Result<GetBrandsQuery>.Failure(nextCursorResult.Error);
            }

            next = nextCursorResult.Value;
        }

        var pagination = new KeysetPagination<BrandId>(limit, prev, next);

        return Result<GetBrandsQuery>.Success(new GetBrandsQuery(search, pagination));
    }
}
