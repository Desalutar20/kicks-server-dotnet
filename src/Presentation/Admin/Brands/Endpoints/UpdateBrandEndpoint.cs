using Application.Admin.Brands.UseCases.UpdateBrand;
using Application.Auth.Types;
using Domain.Brands;

namespace Presentation.Admin.Brands.Endpoints;

public sealed record UpdateBrandRequest(string Name);

public sealed class UpdateBrandRequestValidator : AbstractValidator<UpdateBrandRequest>
{
    public UpdateBrandRequestValidator()
    {
        RuleFor(x => x.Name).ValidateValueObject(BrandName.Create);
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

                    var command = request.ToCommand(id);
                    var result = await commandHandler.Handle(command, ct);

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

    private static UpdateBrandCommand ToCommand(this UpdateBrandRequest request, Guid id)
    {
        var nameResult = BrandName.Create(request.Name).Value;

        return new UpdateBrandCommand(new BrandId(id), nameResult);
    }
}
