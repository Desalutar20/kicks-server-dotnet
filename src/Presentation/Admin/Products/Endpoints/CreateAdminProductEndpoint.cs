using Application.Admin.Products.ProductSkus.Types;
using Application.Admin.Products.Types;
using Application.Admin.Products.UseCases.CreateProduct;
using Application.Auth.Types;
using Domain.Brands;
using Domain.Categories;
using Domain.Products;
using Presentation.Shared.Extensions;

namespace Presentation.Admin.Products.Endpoints;

public sealed record CreateProductRequest(
    string Title,
    string Description,
    string Gender,
    List<string> Tags,
    string CategoryId,
    string BrandId
);

public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Title).ValidateValueObject(ProductTitle.Create);
        RuleFor(x => x.Description).ValidateValueObject(ProductDescription.Create);
        RuleFor(x => x.Gender)
            .NotEmpty()
            .Must(g => Enum.TryParse<ProductGender>(g, true, out _))
            .WithMessage(
                $"Gender must be one of: {string.Join(", ", Enum.GetNames<ProductGender>())}"
            );
        RuleFor(x => x.Tags).NotNull().ValidateValueObject(ProductTags.Create);
        RuleFor(x => x.CategoryId)
            .Must(x => Guid.TryParse(x, out _))
            .WithMessage("Invalid category id");
        RuleFor(x => x.BrandId).Must(x => Guid.TryParse(x, out _)).WithMessage("Invalid brand id");
    }
}

internal static partial class AdminProductsEndpoints
{
    private static IEndpointRouteBuilder CreateProductV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "/",
                async (
                    HttpContext ctx,
                    CreateProductRequest request,
                    ICommandHandler<CreateProductCommand, AdminProductResponse> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.CreateProduct");

                    var command = request.ToCommand();
                    var result = await commandHandler.Handle(command, ct);

                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
                        : Results.Created("/", new ApiResponse<AdminProductResponse>(result.Value));
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter(new AuthorizeFilter(Role.Admin))
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<AdminProductResponse>>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("CreateProduct")
            .WithSummary("Creates a new product.")
            .WithDescription(
                "Creates a new product in the admin panel. Requires admin privileges and validates the provided product data before creation."
            );

        return endpoint;
    }

    private static CreateProductCommand ToCommand(this CreateProductRequest request)
    {
        var title = ProductTitle.Create(request.Title).Value;
        var description = ProductDescription.Create(request.Description).Value;
        var tags = ProductTags.Create(request.Tags).Value;

        return new CreateProductCommand(
            title,
            description,
            Enum.Parse<ProductGender>(request.Gender, true),
            tags,
            new BrandId(Guid.Parse(request.BrandId)),
            new CategoryId(Guid.Parse(request.CategoryId))
        );
    }
}
