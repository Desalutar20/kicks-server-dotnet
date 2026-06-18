using Application.Admin.Products.UseCases.UpdateProduct;
using Application.Auth.Types;
using Domain.Brands;
using Domain.Categories;
using Domain.Products;
using Presentation.Admin.Products.Dto;
using Presentation.Shared.Extensions;

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
        RuleFor(x => x.Title).ValidateNullableValueObject(ProductTitle.Create);
        RuleFor(x => x.Description).ValidateNullableValueObject(ProductDescription.Create);

        RuleFor(x => x.Gender)
            .Must(g => g is null || Enum.TryParse<ProductGender>(g, true, out _))
            .WithMessage(
                $"Gender must be one of: {string.Join(", ", Enum.GetNames<ProductGender>())}"
            );

        RuleFor(x => x.Tags).ValidateNullableValueObject(ProductTags.Create);

        RuleFor(x => x.CategoryId)
            .Must(x => x is null || Guid.TryParse(x, out _))
            .WithMessage("Invalid category id");

        RuleFor(x => x.BrandId)
            .Must(x => x is null || Guid.TryParse(x, out _))
            .WithMessage("Invalid brand id");
    }
}

internal static partial class AdminProductsEndpoints
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

                    var command = request.ToCommand(id);
                    var result = await commandHandler.Handle(command, ct);

                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
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

    private static UpdateProductCommand ToCommand(this UpdateProductRequest request, Guid productId)
    {
        var title = request.Title is not null ? ProductTitle.Create(request.Title).Value : null;
        var description = request.Description is not null
            ? ProductDescription.Create(request.Description).Value
            : null;
        ProductGender? productGender = request.Gender is not null
            ? Enum.Parse<ProductGender>(request.Gender, true)
            : null;

        var tags = request.Tags is not null ? ProductTags.Create(request.Tags).Value : null;
        var brandId = request.BrandId is not null ? new BrandId(Guid.Parse(request.BrandId)) : null;
        var categoryId = request.CategoryId is not null
            ? new CategoryId(Guid.Parse(request.CategoryId))
            : null;

        return new UpdateProductCommand(
            new ProductId(productId),
            title,
            description,
            productGender,
            tags,
            brandId,
            categoryId
        );
    }
}
