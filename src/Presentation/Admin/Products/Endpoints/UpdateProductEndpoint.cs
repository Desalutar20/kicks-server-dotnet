using Application.Admin.Products.UseCases.UpdateProduct;
using Application.Auth.Types;
using Domain.Brand;
using Domain.Category;
using Domain.Product;
using Presentation.Admin.Products.Dto;
using Presentation.Shared;

namespace Presentation.Admin.Products.Endpoints;

public sealed record UpdateProductRequest(
    string? Title,
    string? Description,
    string? Gender,
    List<string>? Tags,
    string? BrandId,
    string? CategoryId
);

public sealed class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Title).MaximumLength(ProductTitle.MaxLength);
        RuleFor(x => x.Description).MaximumLength(ProductDescription.MaxLength);
        RuleFor(x => x.Gender)
            .Must(g => g is null || Enum.TryParse<ProductGender>(g, true, out _))
            .WithMessage("'Gender' must be one of Men, Women, Unisex");
        RuleFor(x => x.Tags)
            .Must(t => t is null || t.Count <= ProductTags.MaxTags)
            .WithMessage($"Tags cannot contain more than {ProductTags.MaxTags} items.");
    }
}

internal static partial class AdminProductSkusEndpoints
{
    private static IEndpointRouteBuilder UpdateProductV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPatch(
                "/{id:guid}",
                async (
                    HttpContext ctx,
                    Guid id,
                    UpdateProductRequest request,
                    ICommandHandler<UpdateProductCommand, Product> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.UpdateProduct");

                    var commandResult = request.ToCommand(id);
                    if (commandResult.IsFailure)
                    {
                        return ErrorHandler.Handle(commandResult.Error, logger);
                    }

                    var result = await commandHandler.Handle(commandResult.Value, ct);
                    return result.IsFailure
                        ? ErrorHandler.Handle(result.Error, logger)
                        : Results.Ok(new ApiResponse<AdminProductDto>(result.Value.ToDto()));
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter(new AuthorizeFilter(Role.Admin))
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<AdminProductDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("UpdateProduct")
            .WithSummary("Updates a product.")
            .WithDescription(
                "Updates a product in the admin panel. Requires admin privileges and validates the provided product data before creation."
            );

        return endpoint;
    }

    private static Result<UpdateProductCommand> ToCommand(
        this UpdateProductRequest request,
        Guid productId
    )
    {
        ProductTitle? title = null;
        ProductDescription? description = null;
        ProductGender? productGender = null;
        ProductTags? tags = null;
        BrandId? brandId = null;
        CategoryId? categoryId = null;

        if (request.Title is not null)
        {
            var titleResult = ProductTitle.Create(request.Title);
            if (titleResult.IsFailure)
            {
                return Result<UpdateProductCommand>.Failure(titleResult.Error);
            }

            title = titleResult.Value;
        }

        if (request.Description is not null)
        {
            var descriptionResult = ProductDescription.Create(request.Description);
            if (descriptionResult.IsFailure)
            {
                return Result<UpdateProductCommand>.Failure(descriptionResult.Error);
            }

            description = descriptionResult.Value;
        }

        if (request.Gender is not null)
        {
            if (!Enum.TryParse<ProductGender>(request.Gender, true, out var gender))
            {
                return Result<UpdateProductCommand>.Failure(
                    Error.Validation("gender", ["Invalid gender"])
                );
            }

            productGender = gender;
        }

        if (request.Tags is not null)
        {
            var tagsResult = ProductTags.Create(request.Tags);
            if (tagsResult.IsFailure)
            {
                return Result<UpdateProductCommand>.Failure(tagsResult.Error);
            }

            tags = tagsResult.Value;
        }

        if (request.BrandId is not null)
        {
            if (!Guid.TryParse(request.BrandId, out var id))
            {
                return Result<UpdateProductCommand>.Failure(
                    Error.Validation("brandId", ["Invalid brand id"])
                );
            }

            brandId = new BrandId(id);
        }

        if (request.CategoryId is not null)
        {
            if (!Guid.TryParse(request.CategoryId, out var id))
            {
                return Result<UpdateProductCommand>.Failure(
                    Error.Validation("categoryId", ["Invalid category id"])
                );
            }

            categoryId = new CategoryId(id);
        }

        return Result<UpdateProductCommand>.Success(
            new UpdateProductCommand(
                new ProductId(productId),
                title,
                description,
                productGender,
                tags,
                brandId,
                categoryId
            )
        );
    }
}
