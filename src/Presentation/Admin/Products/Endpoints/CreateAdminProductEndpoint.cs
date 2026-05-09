using Application.Admin.Products.UseCases.CreateProduct;
using Application.Auth.Types;
using Domain.Product;
using Domain.Product.Brand;
using Domain.Product.Category;
using Presentation.Admin.Products.Dto;

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
        RuleFor(x => x.Title).NotEmpty().MaximumLength(ProductTitle.MaxLength);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(ProductDescription.MaxLength);
        RuleFor(x => x.Gender)
            .NotEmpty()
            .Must(g => Enum.TryParse<ProductGender>(g, true, out _))
            .WithMessage("'Gender' must be one of Men, Women, Unisex");
        RuleFor(x => x.Tags)
            .NotNull()
            .Must(t => t is not null && t.Count <= ProductTags.MaxTags)
            .WithMessage($"Tags cannot contain more than {ProductTags.MaxTags} items.");
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.BrandId).NotEmpty();
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
                    ICommandHandler<CreateProductCommand, Product> commandHandler,
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

                    var commandResult = request.ToCommand();
                    if (commandResult.IsFailure)
                    {
                        return ErrorHandler.Handle(commandResult.Error, logger);
                    }

                    var result = await commandHandler.Handle(commandResult.Value, ct);
                    return result.IsFailure
                        ? ErrorHandler.Handle(result.Error, logger)
                        : Results.Created("/", new ApiResponse<ProductDto>(result.Value.ToDto()));
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter(new AuthorizeFilter(Role.Admin))
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<ProductDto>>(StatusCodes.Status201Created)
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

    private static Result<CreateProductCommand> ToCommand(this CreateProductRequest request)
    {
        var title = ProductTitle.Create(request.Title);
        if (title.IsFailure)
        {
            return Result<CreateProductCommand>.Failure(title.Error);
        }

        var description = ProductDescription.Create(request.Description);
        if (description.IsFailure)
        {
            return Result<CreateProductCommand>.Failure(description.Error);
        }

        if (!Enum.TryParse<ProductGender>(request.Gender, true, out var productGender))
        {
            return Result<CreateProductCommand>.Failure(
                Error.Validation("gender", ["Invalid gender"])
            );
        }

        var tags = ProductTags.Create(request.Tags);
        if (tags.IsFailure)
        {
            return Result<CreateProductCommand>.Failure(tags.Error);
        }

        if (!Guid.TryParse(request.BrandId, out var brandId))
        {
            return Result<CreateProductCommand>.Failure(
                Error.Validation("brandId", ["Invalid brand id"])
            );
        }

        if (!Guid.TryParse(request.CategoryId, out var categoryId))
        {
            return Result<CreateProductCommand>.Failure(
                Error.Validation("categoryId", ["Invalid category id"])
            );
        }

        return Result<CreateProductCommand>.Success(
            new CreateProductCommand(
                title.Value,
                description.Value,
                productGender,
                tags.Value,
                new BrandId(brandId),
                new CategoryId(categoryId)
            )
        );
    }
}
