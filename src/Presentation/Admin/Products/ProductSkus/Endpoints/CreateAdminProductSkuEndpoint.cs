using Application.Admin.Products.ProductSkus.Constants;
using Application.Admin.Products.ProductSkus.UseCases.CreateProductSku;
using Application.Auth.Types;
using Domain.Product;
using Domain.Product.ProductSku;
using Presentation.Shared;
using File = Application.Abstractions.FileUploader.File;

namespace Presentation.Admin.Products.ProductSkus.Endpoints;

public sealed record CreateProductSkuRequest
{
    [FromForm(Name = "price")]
    public int Price { get; set; }

    [FromForm(Name = "salePrice")]
    public int? SalePrice { get; set; }

    [FromForm(Name = "quantity")]
    public int Quantity { get; set; }

    [FromForm(Name = "size")]
    public int Size { get; set; }

    [FromForm(Name = "color")]
    public string Color { get; set; } = null!;

    [FromForm(Name = "sku")]
    public string Sku { get; set; } = null!;

    [FromForm(Name = "images")]
    public IFormFileCollection Images { get; set; } = null!;
}

public sealed class CreateProductSkuRequestValidator : AbstractValidator<CreateProductSkuRequest>
{
    public CreateProductSkuRequestValidator()
    {
        RuleFor(x => x.Price).NotEmpty().GreaterThan(0);
        RuleFor(x => x)
            .Must(x => x.SalePrice == null || x.SalePrice < x.Price)
            .WithMessage("Sale price must be less than price.")
            .WithName("salePrice");

        RuleFor(x => x.Quantity).NotEmpty().GreaterThan(0);
        RuleFor(x => x.Size).NotEmpty().GreaterThan(0);

        RuleFor(x => x.Color).NotEmpty();
        RuleFor(x => x.Sku).NotEmpty();

        RuleFor(x => x.Images)
            .NotNull()
            .NotEmpty()
            .Must(x => x != null && x.Count <= ProductSku.MaxImages)
            .WithMessage($"Max {ProductSku.MaxImages} images allowed.");

        RuleFor(x => x.Images)
            .Must(x =>
                x.All(file => ProductSkusConstants.AllowedContentTypes.Contains(file.ContentType))
            )
            .WithMessage("Only JPEG, PNG, and WEBP images are allowed.");

        RuleFor(x => x.Images)
            .Must(x =>
                x.All(file =>
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
    private static IEndpointRouteBuilder CreateProductSkuV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "/{productId:guid}/skus",
                async (
                    HttpContext ctx,
                    [FromForm] CreateProductSkuRequest request,
                    Guid productId,
                    ICommandHandler<CreateProductSkuCommand, ProductSkuId> commandHandler,
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

                    var logger = loggerFactory.CreateLogger("Admin.CreateProductSku");

                    var commandResult = request.ToCommand(productId);
                    if (commandResult.IsFailure)
                    {
                        return ErrorHandler.Handle(commandResult.Error, logger);
                    }

                    var result = await commandHandler.Handle(commandResult.Value, ct);
                    return result.IsFailure
                        ? ErrorHandler.Handle(result.Error, logger)
                        : Results.Created("/", new ApiResponse<Guid>(result.Value.Value));
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter(new AuthorizeFilter(Role.Admin))
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("CreateProductSku")
            .WithSummary("Creates a new product sku.")
            .WithDescription(
                "Creates a new product  sku in the admin panel. Requires admin privileges and validates the provided product data before creation."
            )
            .DisableAntiforgery();

        return endpoint;
    }

    private static Result<CreateProductSkuCommand> ToCommand(
        this CreateProductSkuRequest request,
        Guid productId
    )
    {
        var positivePrice = PositiveInt.Create(request.Price, "price");
        if (positivePrice.IsFailure)
        {
            return Result<CreateProductSkuCommand>.Failure(positivePrice.Error);
        }

        PositiveInt? positiveSalePrice = null;
        if (request.SalePrice is not null)
        {
            var positiveSalePriceResult = PositiveInt.Create(request.SalePrice.Value, "salePrice");
            if (positiveSalePriceResult.IsFailure)
            {
                return Result<CreateProductSkuCommand>.Failure(positiveSalePriceResult.Error);
            }

            positiveSalePrice = positiveSalePriceResult.Value;
        }

        var price = ProductSkuPrice.Create(positivePrice.Value, positiveSalePrice);
        if (price.IsFailure)
        {
            return Result<CreateProductSkuCommand>.Failure(price.Error);
        }

        var quantity = PositiveInt.Create(request.Quantity);
        if (quantity.IsFailure)
        {
            return Result<CreateProductSkuCommand>.Failure(quantity.Error);
        }

        var size = PositiveInt.Create(request.Size);
        if (size.IsFailure)
        {
            return Result<CreateProductSkuCommand>.Failure(size.Error);
        }

        var color = ProductSkuColor.Create(request.Color);
        if (color.IsFailure)
        {
            return Result<CreateProductSkuCommand>.Failure(color.Error);
        }

        var sku = ProductSkuSku.Create(request.Sku);
        if (sku.IsFailure)
        {
            return Result<CreateProductSkuCommand>.Failure(sku.Error);
        }

        List<File> files = [];

        foreach (var image in request.Images)
        {
            var fileName = NonEmptyString.Create(image.FileName);
            if (fileName.IsFailure)
            {
                return Result<CreateProductSkuCommand>.Failure(fileName.Error);
            }

            var contentType = NonEmptyString.Create(image.ContentType);
            if (contentType.IsFailure)
            {
                return Result<CreateProductSkuCommand>.Failure(contentType.Error);
            }

            var stream = image.OpenReadStream();

            files.Add(new File(stream, fileName.Value, contentType.Value));
        }

        return Result<CreateProductSkuCommand>.Success(
            new CreateProductSkuCommand(
                price.Value,
                quantity.Value,
                size.Value,
                color.Value,
                sku.Value,
                new ProductId(productId),
                files
            )
        );
    }
}
