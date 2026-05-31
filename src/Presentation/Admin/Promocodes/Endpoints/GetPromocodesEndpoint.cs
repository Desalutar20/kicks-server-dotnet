using Application.Admin.Promocodes.Constants;
using Application.Admin.Promocodes.UseCases.GetPromocodes;
using Application.Auth.Types;
using Domain.Promocodes;
using Presentation.Admin.Promocodes.Dto;

namespace Presentation.Admin.Promocodes.Endpoints;

public sealed record GetPromocodesRequest(
    string? Code,
    int? Limit,
    string? PrevCursor,
    string? NextCursor
);

public sealed class GetPromocodesRequestValidator : AbstractValidator<GetPromocodesRequest>
{
    public GetPromocodesRequestValidator()
    {
        RuleFor(x => x.Code).ValidateNullableValueObject(PromocodeCode.Create);

        RuleFor(x => x.Limit).InclusiveBetween(1, PromocodesConstants.GetPromocodesMaxLimit);

        RuleFor(x => x)
            .Must(x => x.PrevCursor is null || x.NextCursor is null)
            .WithMessage("Only one cursor can be specified: PrevCursor or NextCursor.")
            .WithName("prevCursor");

        RuleFor(x => x.PrevCursor)
            .ValidateNullableValueObject(x =>
                KeysetCursor<PromocodeId>.Create(
                    x,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid promocode id")
                            : new PromocodeId(id)
                )
            )
            .MaximumLength(PromocodesConstants.GetPromocodesCursorMaxLength);

        RuleFor(x => x.NextCursor)
            .ValidateNullableValueObject(x =>
                KeysetCursor<PromocodeId>.Create(
                    x,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid promocode id")
                            : new PromocodeId(id)
                )
            )
            .MaximumLength(PromocodesConstants.GetPromocodesCursorMaxLength);
    }
}

internal static partial class AdminPromocodesEndpoints
{
    private static IEndpointRouteBuilder GetPromocodesV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet(
                "/",
                async (
                    HttpContext ctx,
                    IQueryHandler<
                        GetPromocodesQuery,
                        KeysetPaginated<Promocode, PromocodeId>
                    > queryHandler,
                    [AsParameters] GetPromocodesRequest request,
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

                    var logger = loggerFactory.CreateLogger("Admin.GetPromocodes");

                    var query = request.ToQuery();
                    var result = await queryHandler.Handle(query, ct);

                    if (result.IsFailure)
                    {
                        return ErrorHandler.Handle(result.Error, logger);
                    }

                    return Results.Ok(
                        new ApiCursorResponse<AdminPromocodeDto>(
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
            .Produces<ApiCursorResponse<AdminPromocodeDto>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("GetPromocodes")
            .WithSummary("Retrieves a paginated list of promocodes for admin panel.")
            .WithDescription(
                "Returns a filtered and paginated list of promocodes. Supports search by code and keyset pagination for efficient navigation through large datasets."
            );

        return endpoint;
    }

    private static GetPromocodesQuery ToQuery(this GetPromocodesRequest request)
    {
        var limit = PositiveInt
            .Create(request.Limit ?? PromocodesConstants.GetPromocodesDefaultLimit)
            .Value;

        var code = request.Code is not null ? PromocodeCode.Create(request.Code).Value : null;

        var prev = request.PrevCursor is not null
            ? KeysetCursor<PromocodeId>
                .Create(
                    request.PrevCursor,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid promocode id")
                            : new PromocodeId(id)
                )
                .Value
            : null;

        var next = request.NextCursor is not null
            ? KeysetCursor<PromocodeId>
                .Create(
                    request.NextCursor,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid promocode id")
                            : new PromocodeId(id)
                )
                .Value
            : null;

        var pagination = new KeysetPagination<PromocodeId>(limit, prev, next);

        return new GetPromocodesQuery(code, pagination);
    }
}
