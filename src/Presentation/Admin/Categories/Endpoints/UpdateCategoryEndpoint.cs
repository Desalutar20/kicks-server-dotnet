using Application.Admin.Categories.UseCases.UpdateCategory;
using Application.Auth.Types;
using Domain.Category;
using Presentation.Shared;

namespace Presentation.Admin.Categories.Endpoints;

public sealed record UpdateCategoryRequest(string Name);

public sealed class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(CategoryName.MaxLength);
    }
}

internal static partial class AdminCategoriesEndpoints
{
    private static IEndpointRouteBuilder UpdateCategoryV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPatch(
                "/{id:guid}",
                async (
                    HttpContext ctx,
                    UpdateCategoryRequest request,
                    Guid id,
                    ICommandHandler<UpdateCategoryCommand> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.UpdateCategory");

                    var commandResult = request.ToCommand(id);
                    if (commandResult.IsFailure)
                    {
                        return ErrorHandler.Handle(commandResult.Error, logger);
                    }

                    var result = await commandHandler.Handle(commandResult.Value, ct);
                    return result.IsFailure
                        ? ErrorHandler.Handle(result.Error, logger)
                        : Results.Ok(new ApiResponse<string>("success"));
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter(new AuthorizeFilter(Role.Admin))
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<string>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("UpdateCategory")
            .WithSummary("Update an existing category.")
            .WithDescription("Update an existing category. Requires admin privileges.");

        return endpoint;
    }

    private static Result<UpdateCategoryCommand> ToCommand(
        this UpdateCategoryRequest request,
        Guid id
    )
    {
        var nameResult = CategoryName.Create(request.Name);
        return nameResult.IsFailure
            ? Result<UpdateCategoryCommand>.Failure(nameResult.Error)
            : Result<UpdateCategoryCommand>.Success(
                new UpdateCategoryCommand(new CategoryId(id), nameResult.Value)
            );
    }
}
