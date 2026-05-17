using Application.Admin.Brands.UseCases.UpdateBrand;
using Application.Auth.Types;
using Domain.Brand;
using Presentation.Shared;

namespace Presentation.Admin.Brands.Endpoints;

public sealed record UpdateBrandRequest(string Name);

public sealed class UpdateBrandRequestValidator : AbstractValidator<UpdateBrandRequest>
{
    public UpdateBrandRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(BrandName.MaxLength);
    }
}

internal static partial class AdminBrandsEndpoints
{
    private static IEndpointRouteBuilder UpdateBrandV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPatch(
                "/{id:guid}",
                async (
                    HttpContext ctx,
                    UpdateBrandRequest request,
                    Guid id,
                    ICommandHandler<UpdateBrandCommand> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.UpdateBrand");

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
            .WithName("UpdateBrand")
            .WithSummary("Update an existing brand.")
            .WithDescription("Update an existing brand. Requires admin privileges.");

        return endpoint;
    }

    private static Result<UpdateBrandCommand> ToCommand(this UpdateBrandRequest request, Guid id)
    {
        var nameResult = BrandName.Create(request.Name);
        return nameResult.IsFailure
            ? Result<UpdateBrandCommand>.Failure(nameResult.Error)
            : Result<UpdateBrandCommand>.Success(
                new UpdateBrandCommand(new BrandId(id), nameResult.Value)
            );
    }
}
