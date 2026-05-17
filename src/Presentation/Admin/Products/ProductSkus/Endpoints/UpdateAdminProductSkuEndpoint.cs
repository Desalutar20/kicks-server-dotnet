using Application.Admin.Products.ProductSkus.Constants;
using Application.Admin.Products.ProductSkus.UseCases.UpdateProductSku;
using Application.Auth.Types;
using Domain.Product.ProductSku;
using Presentation.Admin.Products.ProductSkus.Dto;
using Presentation.Shared;
using File = Application.Abstractions.FileUploader.File;

namespace Presentation.Admin.Products.ProductSkus.Endpoints;

public sealed record UpdateProductSkuRequest
{
    [FromForm(Name = "price")]
    public int? Price { get; set; }

    [FromForm(Name = "salePrice")]
    public int? SalePrice { get; set; }

    [FromForm(Name = "quantity")]
    public int? Quantity { get; set; }

    [FromForm(Name = "size")]
    public int? Size { get; set; }

    [FromForm(Name = "color")]
    public string? Color { get; set; }

    [FromForm(Name = "sku")]
    public string? Sku { get; set; }

    [FromForm(Name = "images")]
    public IFormFileCollection? Images { get; set; }
}

public sealed class UpdateProductSkuRequestValidator : AbstractValidator<UpdateProductSkuRequest>
{
    public UpdateProductSkuRequestValidator()
    {
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x)
            .Must(x => x.SalePrice == null || x.Price is null || x.SalePrice < x.Price)
            .WithMessage("Sale price must be less than price.")
            .WithName("salePrice");

        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Size).GreaterThan(0);

        RuleFor(x => x.Images)
            .Must(x => x is null || x.Count <= ProductSku.MaxImages)
            .WithMessage($"Max {ProductSku.MaxImages} images allowed.");

        RuleFor(x => x.Images)
            .Must(x =>
                x is null
                || x.All(file =>
                    ProductSkusConstants.AllowedContentTypes.Contains(file.ContentType)
                )
            )
            .WithMessage("Only JPEG, PNG, and WEBP images are allowed.");

        RuleFor(x => x.Images)
            .Must(x =>
                x is null
                || x.All(file =>
                    file.Length > 0 && file.Length <= ProductSkusConstants.MaxFileSizeBytes
                )
            )
            .WithMessage(
                $"Image size cannot exceed {ProductSkusConstants.MaxFileSizeBytes / 1024 / 1024} MB."
            );
    }
}

internal static partial class AdminProductSkusEndpoints
{
    private static IEndpointRouteBuilder UpdateProductSkuV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPatch(
                "/skus/{id:guid}",
                async (
                    HttpContext ctx,
                    [FromForm] UpdateProductSkuRequest request,
                    Guid id,
                    ICommandHandler<UpdateProductSkuCommand, ProductSku> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.UpdateProductSku");

                    var commandResult = request.ToCommand(id);
                    if (commandResult.IsFailure)
                    {
                        return ErrorHandler.Handle(commandResult.Error, logger);
                    }

                    var result = await commandHandler.Handle(commandResult.Value, ct);
                    return result.IsFailure
                        ? ErrorHandler.Handle(result.Error, logger)
                        : Results.Ok(new ApiResponse<AdminProductSkuDto>(result.Value.ToDto()));
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter(new AuthorizeFilter(Role.Admin))
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<AdminProductSkuDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("UpdateProductSku")
            .WithSummary("Update product sku.")
            .WithDescription(
                "Update a product sku in the admin panel. Requires admin privileges and validates the provided product data before creation."
            )
            .DisableAntiforgery();

        return endpoint;
    }

    private static Result<UpdateProductSkuCommand> ToCommand(
        this UpdateProductSkuRequest request,
        Guid productSkuId
    )
    {
        PositiveInt? price = null;
        PositiveInt? salePrice = null;
        PositiveInt? quantity = null;
        PositiveInt? size = null;
        ProductSkuSku? sku = null;
        ProductSkuColor? color = null;
        List<File>? files = null;

        if (request.Price is not null)
        {
            var priceResult = PositiveInt.Create(request.Price.Value, "price");
            if (priceResult.IsFailure)
            {
                return Result<UpdateProductSkuCommand>.Failure(priceResult.Error);
            }

            price = priceResult.Value;
        }

        if (request.SalePrice is not null)
        {
            var salePriceResult = PositiveInt.Create(request.SalePrice.Value, "salePrice");
            if (salePriceResult.IsFailure)
            {
                return Result<UpdateProductSkuCommand>.Failure(salePriceResult.Error);
            }

            salePrice = salePriceResult.Value;
        }

        if (request.Quantity is not null)
        {
            var quantityResult = PositiveInt.Create(request.Quantity.Value);
            if (quantityResult.IsFailure)
            {
                return Result<UpdateProductSkuCommand>.Failure(quantityResult.Error);
            }

            quantity = quantityResult.Value;
        }

        if (request.Size is not null)
        {
            var sizeResult = PositiveInt.Create(request.Size.Value);
            if (sizeResult.IsFailure)
            {
                return Result<UpdateProductSkuCommand>.Failure(sizeResult.Error);
            }

            size = sizeResult.Value;
        }

        if (request.Sku is not null)
        {
            var skuResult = ProductSkuSku.Create(request.Sku);
            if (skuResult.IsFailure)
            {
                return Result<UpdateProductSkuCommand>.Failure(skuResult.Error);
            }

            sku = skuResult.Value;
        }

        if (request.Color is not null)
        {
            var colorResult = ProductSkuColor.Create(request.Color);
            if (colorResult.IsFailure)
            {
                return Result<UpdateProductSkuCommand>.Failure(colorResult.Error);
            }

            color = colorResult.Value;
        }

        if (request.Images is not null)
        {
            files = [];

            foreach (var image in request.Images)
            {
                var fileName = NonEmptyString.Create(image.FileName);
                if (fileName.IsFailure)
                {
                    return Result<UpdateProductSkuCommand>.Failure(fileName.Error);
                }

                var contentType = NonEmptyString.Create(image.ContentType);
                if (contentType.IsFailure)
                {
                    return Result<UpdateProductSkuCommand>.Failure(contentType.Error);
                }

                var stream = image.OpenReadStream();

                files.Add(new File(stream, fileName.Value, contentType.Value));
            }
        }

        return Result<UpdateProductSkuCommand>.Success(
            new UpdateProductSkuCommand(
                new ProductSkuId(productSkuId),
                price,
                salePrice,
                quantity,
                size,
                color,
                sku,
                files
            )
        );
    }
}
