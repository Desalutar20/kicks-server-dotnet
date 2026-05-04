using System.Net;
using Application.Admin.Brands.UseCases.CreateBrand;
using Application.Auth.Types;
using Domain.Product.Brand;

namespace Presentation.Admin.Brands.Endpoints;

public sealed record CreateBrandRequest(string Name);

public sealed class CreateBrandRequestValidator : AbstractValidator<CreateBrandRequest>
{
    public CreateBrandRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(BrandName.MaxLength);
    }
}

internal static partial class AdminBrandsEndpoints
{
    private static IEndpointRouteBuilder CreateBrandV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "/",
                async (
                    HttpContext ctx,
                    CreateBrandRequest request,
                    ICommandHandler<CreateBrandCommand> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.CreateBrand");

                    var commandResult = request.ToCommand();
                    if (commandResult.IsFailure)
                    {
                        return ErrorHandler.Handle(commandResult.Error, logger);
                    }

                    var result = await commandHandler.Handle(commandResult.Value, ct);
                    return result.IsFailure
                        ? ErrorHandler.Handle(result.Error, logger)
                        : Results.Created("/", new ApiResponse<string>("success"));
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter(new AuthorizeFilter(Role.Admin))
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<string>>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("CreateBrand")
            .WithSummary("Creates a new brand.")
            .WithDescription("Creates a new brand. Requires admin privileges.");

        return endpoint;
    }

    private static Result<CreateBrandCommand> ToCommand(this CreateBrandRequest request)
    {
        var nameResult = BrandName.Create(request.Name);
        return nameResult.IsFailure
            ? Result<CreateBrandCommand>.Failure(nameResult.Error)
            : Result<CreateBrandCommand>.Success(new CreateBrandCommand(nameResult.Value));
    }
}
