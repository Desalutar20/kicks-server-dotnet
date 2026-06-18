using Application.Auth.Types;
using Application.Orders.Constants;
using Application.Orders.UseCases.GetOrders;
using Domain.Orders;
using Domain.Shared.ValueObjects;
using Presentation.Orders.Dto;
using Presentation.Shared.Extensions;

namespace Presentation.Orders.Endpoints;

public sealed record GetOrdersRequest(int? Limit, string? PrevCursor, string? NextCursor);

public sealed class GetOrdersRequestValidator : AbstractValidator<GetOrdersRequest>
{
    public GetOrdersRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x.PrevCursor is null || x.NextCursor is null)
            .WithMessage("Only one cursor can be specified: PrevCursor or NextCursor.")
            .WithName("prevCursor");

        RuleFor(x => x.Limit).InclusiveBetween(1, OrderConstants.GetOrdersMaxLimit);

        RuleFor(x => x.PrevCursor)
            .ValidateNullableValueObject(x =>
                KeysetCursor<OrderId>.Create(
                    x,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid order id")
                            : new OrderId(id)
                )
            )
            .MaximumLength(OrderConstants.GetOrdersCursorMaxLength);

        RuleFor(x => x.NextCursor)
            .ValidateNullableValueObject(x =>
                KeysetCursor<OrderId>.Create(
                    x,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid order id")
                            : new OrderId(id)
                )
            )
            .MaximumLength(OrderConstants.GetOrdersCursorMaxLength);
    }
}

internal static partial class OrderEndpoints
{
    private static IEndpointRouteBuilder GetOrdersV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet(
                "",
                async (
                    [AsParameters] GetOrdersRequest request,
                    IQueryHandler<GetOrdersQuery, KeysetPaginated<Order, OrderId>> queryHandler,
                    HttpContext ctx,
                    ILoggerFactory loggerFactory,
                    CancellationToken ct
                ) =>
                {
                    if (
                        !ctx.Items.TryGetValue(RequestConstants.UserKey, out var user)
                        || user is not SessionUser sessionUser
                    )
                    {
                        return Results.Unauthorized();
                    }

                    var logger = loggerFactory.CreateLogger("GetORders");

                    var query = request.ToQuery(sessionUser.Id);
                    var result = await queryHandler.Handle(query, ct);

                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
                        : Results.Ok(
                            new ApiCursorResponse<OrderDto>(
                                [.. result.Value.Data.Select(u => u.ToDto())],
                                result.Value.PrevCursor?.ToString(),
                                result.Value.NextCursor?.ToString()
                            )
                        );
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiCursorResponse<OrderDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("GetOrders")
            .WithSummary("Get user's orders")
            .WithDescription(
                "Returns a paginated list of orders belonging to the authenticated user. Supports keyset pagination using PrevCursor and NextCursor."
            )
            .RequireRateLimiting(RateLimitConstants.GetOrders);

        return endpoint;
    }

    private static GetOrdersQuery ToQuery(this GetOrdersRequest request, Guid userId)
    {
        var limit = PositiveInt.Create(request.Limit ?? OrderConstants.GetOrdersDefaultLimit).Value;

        var prev = request.PrevCursor is not null
            ? KeysetCursor<OrderId>
                .Create(
                    request.PrevCursor,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid order id")
                            : new OrderId(id)
                )
                .Value
            : null;

        var next = request.NextCursor is not null
            ? KeysetCursor<OrderId>
                .Create(
                    request.NextCursor,
                    s =>
                        !Guid.TryParse(s, out var id)
                            ? Error.Failure("Invalid order id")
                            : new OrderId(id)
                )
                .Value
            : null;

        var pagination = new KeysetPagination<OrderId>(limit, prev, next);

        return new GetOrdersQuery(new UserId(userId), pagination);
    }
}
