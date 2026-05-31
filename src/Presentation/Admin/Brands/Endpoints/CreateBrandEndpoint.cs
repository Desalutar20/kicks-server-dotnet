using Application.Admin.Brands.UseCases.CreateBrand;
using Application.Auth.Types;
using Domain.Brands;
using Presentation.Admin.Brands.Dto;

namespace Presentation.Admin.Brands.Endpoints;

public sealed record CreateBrandRequest(string Name);

public sealed class CreateBrandRequestValidator : AbstractValidator<CreateBrandRequest>
{
    public CreateBrandRequestValidator()
    {
        RuleFor(x => x.Name).ValidateValueObject(BrandName.Create);
    }
}

internal static partial class AdminPromocodesEndpoints
{
    private static IEndpointRouteBuilder CreateBrandV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "/",
                async (
                    HttpContext ctx,
                    CreateBrandRequest request,
                    ICommandHandler<CreateBrandCommand, Brand> commandHandler,
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

                    var command = request.ToCommand();
                    var result = await commandHandler.Handle(command, ct);
                    return result.IsFailure
                        ? ErrorHandler.Handle(result.Error, logger)
                        : Results.Created(
                            "/",
                            new ApiResponse<AdminBrandDto>(result.Value.ToDto())
                        );
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter(new AuthorizeFilter(Role.Admin))
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<AdminBrandDto>>(StatusCodes.Status201Created)
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

    private static CreateBrandCommand ToCommand(this CreateBrandRequest request)
    {
        var nameResult = BrandName.Create(request.Name).Value;
        return new CreateBrandCommand(nameResult);
    }
}
