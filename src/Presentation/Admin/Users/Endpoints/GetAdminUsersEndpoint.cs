using Application.Admin.Users.Constants;
using Application.Admin.Users.Types;
using Application.Admin.Users.UseCases.GetAllAdminUsers;
using Application.Auth.Types;
using Presentation.Admin.Users.Dto;

namespace Presentation.Admin.Users.Endpoints;

public sealed record GetAdminUsersRequest(
    string? Search,
    bool? IsBanned,
    bool? IsVerified,
    string? Gender,
    int? Limit,
    string? PrevCursor,
    string? NextCursor
);

public sealed class GetAdminUsersRequestValidator : AbstractValidator<GetAdminUsersRequest>
{
    public GetAdminUsersRequestValidator()
    {
        RuleFor(x => x.Search)
            .ValidateNullableValueObject(x => NonEmptyString.Create(x, label: "Search"))
            .MaximumLength(AdminUsersConstants.GetAdminUsersSearchMaxLength);

        RuleFor(x => x.Gender)
            .Must(g => g is null || Enum.TryParse<Gender>(g, true, out _))
            .WithMessage($"Gender must be one of: {string.Join(", ", Enum.GetNames<Gender>())}");
        RuleFor(x => x.Limit).InclusiveBetween(1, AdminUsersConstants.GetAdminUsersMaxLimit);

        RuleFor(x => x)
            .Must(x => x.PrevCursor is null || x.NextCursor is null)
            .WithMessage("Only one cursor can be specified: PrevCursor or NextCursor.")
            .WithName("prevCursor");

        RuleFor(x => x.PrevCursor)
            .ValidateNullableValueObject(x =>
                KeysetCursor<UserId>.Create(
                    x,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid user id")
                            : new UserId(id)
                )
            )
            .MaximumLength(AdminUsersConstants.GetAdminUsersCursorMaxLength);

        RuleFor(x => x.NextCursor)
            .ValidateNullableValueObject(x =>
                KeysetCursor<UserId>.Create(
                    x,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid user id")
                            : new UserId(id)
                )
            )
            .MaximumLength(AdminUsersConstants.GetAdminUsersCursorMaxLength);

        RuleFor(x => x.NextCursor).MaximumLength(AdminUsersConstants.GetAdminUsersCursorMaxLength);
    }
}

internal static partial class AdminUsersEndpoints
{
    private static IEndpointRouteBuilder GetAdminUsersV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet(
                "/",
                async (
                    HttpContext ctx,
                    IQueryHandler<
                        GetAllAdminUsersQuery,
                        KeysetPaginated<AdminUser, UserId>
                    > queryHandler,
                    [AsParameters] GetAdminUsersRequest request,
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

                    var logger = loggerFactory.CreateLogger("Admin.GetAdminUsers");

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
                        new ApiCursorResponse<AdminUserDto>(
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
            .Produces<ApiCursorResponse<AdminUserDto>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("GetAdminUsers")
            .WithSummary("Retrieves a paginated list of users for admin panel.")
            .WithDescription(
                "Returns a filtered and paginated list of users. Supports search, ban status, verification status, and keyset pagination for efficient navigation through large datasets."
            );

        return endpoint;
    }

    private static Result<GetAllAdminUsersQuery> ToQuery(this GetAdminUsersRequest request)
    {
        var limit = PositiveInt
            .Create(request.Limit ?? AdminUsersConstants.GetAdminUsersDefaultLimit)
            .Value;

        NonEmptyString? search = request.Search is not null
            ? NonEmptyString.Create(request.Search).Value
            : null;
        Gender? gender = request.Gender is not null
            ? Enum.Parse<Gender>(request.Gender, true)
            : null;
        var prev = request.PrevCursor is not null
            ? KeysetCursor<UserId>
                .Create(
                    request.PrevCursor,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid user id")
                            : new UserId(id)
                )
                .Value
            : null;
        var next = request.NextCursor is not null
            ? KeysetCursor<UserId>
                .Create(
                    request.NextCursor,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid user id")
                            : new UserId(id)
                )
                .Value
            : null;

        var filters = new UsersFilters(search, gender, request.IsBanned, request.IsVerified);
        var pagination = new KeysetPagination<UserId>(limit, prev, next);

        return new GetAllAdminUsersQuery(filters, pagination);
    }
}
