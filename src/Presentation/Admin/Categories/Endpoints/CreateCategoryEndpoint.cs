using Application.Admin.Categories.UseCases.CreateCategory;
using Application.Auth.Types;
using Domain.Product.Category;
using Presentation.Admin.Categories.Dto;

namespace Presentation.Admin.Categories.Endpoints;

public sealed record CreateCategoryRequest(string Name);

public sealed class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(CategoryName.MaxLength);
    }
}

internal static partial class AdminCategoriesEndpoints
{
    private static IEndpointRouteBuilder CreateCategoryV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "/",
                async (
                    HttpContext ctx,
                    CreateCategoryRequest request,
                    ICommandHandler<CreateCategoryCommand, Category> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.CreateCategory");

                    var commandResult = request.ToCommand();
                    if (commandResult.IsFailure)
                    {
                        return ErrorHandler.Handle(commandResult.Error, logger);
                    }

                    var result = await commandHandler.Handle(commandResult.Value, ct);
                    return result.IsFailure
                        ? ErrorHandler.Handle(result.Error, logger)
                        : Results.Created("/", new ApiResponse<CategoryDto>(result.Value.ToDto()));
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter(new AuthorizeFilter(Role.Admin))
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<CategoryDto>>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("CreateCategory")
            .WithSummary("Creates a new category.")
            .WithDescription("Creates a new category. Requires admin privileges.");

        return endpoint;
    }

    private static Result<CreateCategoryCommand> ToCommand(this CreateCategoryRequest request)
    {
        var nameResult = CategoryName.Create(request.Name);
        return nameResult.IsFailure
            ? Result<CreateCategoryCommand>.Failure(nameResult.Error)
            : Result<CreateCategoryCommand>.Success(new CreateCategoryCommand(nameResult.Value));
    }
}
