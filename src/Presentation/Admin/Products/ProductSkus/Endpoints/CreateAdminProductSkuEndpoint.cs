using Application.Abstractions.FileUploader;
using Application.Admin.Products.ProductSkus.Constants;
using Application.Admin.Products.ProductSkus.UseCases.CreateProductSku;
using Application.Auth.Types;
using Domain.Products;
using Domain.Products.ProductSkus;
using Domain.Shared.FileContent;

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
        RuleFor(x => x.Price).ValidateValueObject(x => PositiveInt.Create(x, label: "Price"));
        RuleFor(x => x.SalePrice)
            .ValidateNullableValueObject(x => PositiveInt.Create(x!.Value, label: "Sale price"));
        RuleFor(x => x)
            .Must(x => x.SalePrice == null || x.SalePrice < x.Price)
            .WithMessage("Sale price must be less than price.")
            .WithName("salePrice");

        RuleFor(x => x.Quantity).ValidateValueObject(x => PositiveInt.Create(x, label: "Quantity"));
        RuleFor(x => x.Size).ValidateValueObject(x => PositiveInt.Create(x, label: "Size"));

        RuleFor(x => x.Color).ValidateValueObject(ProductSkuColor.Create);
        RuleFor(x => x.Sku).ValidateValueObject(ProductSkuSku.Create);

        RuleFor(x => x.Images)
            .NotNull()
            .NotEmpty()
            .Must(x => x != null && x.Count <= ProductSku.MaxImages)
            .WithMessage($"Max {ProductSku.MaxImages} images allowed.");

        RuleFor(x => x.Images)
            .Must(x =>
                x.All(file =>
                    !string.IsNullOrWhiteSpace(file.ContentType)
                    && ProductSkusConstants.AllowedContentTypes.Contains(file.ContentType)
                )
            )
            .WithMessage("Only JPEG, PNG, and WEBP images are allowed.");

        RuleFor(x => x.Images)
            .Must(x => x is null || x.All(file => file.FileName.Length <= FileName.MaxLength))
            .WithMessage($"File name must not exceed {FileName.MaxLength} characters.");

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

                    var (command, files) = commandResult.Value;

                    try
                    {
                        var result = await commandHandler.Handle(command, ct);
                        return result.IsFailure
                            ? ErrorHandler.Handle(result.Error, logger)
                            : Results.Created("/", new ApiResponse<Guid>(result.Value.Value));
                    }
                    finally
                    {
                        await Task.WhenAll(files.Select(async f => await f.Content.DisposeAsync()));
                    }
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

    private static Result<(CreateProductSkuCommand command, List<FileData> streams)> ToCommand(
        this CreateProductSkuRequest request,
        Guid productId
    )
    {
        var positivePrice = PositiveInt.Create(request.Price, "price").Value;

        PositiveInt? positiveSalePrice = request.SalePrice is not null
            ? PositiveInt.Create(request.SalePrice.Value).Value
            : null;

        var price = ProductSkuPrice.Create(positivePrice, positiveSalePrice);
        if (price.IsFailure)
        {
            return price.Error;
        }

        var quantity = PositiveInt.Create(request.Quantity).Value;
        var size = PositiveInt.Create(request.Size).Value;
        var color = ProductSkuColor.Create(request.Color).Value;
        var sku = ProductSkuSku.Create(request.Sku).Value;

        List<FileData> files = [];

        foreach (var image in request.Images)
        {
            var fileName = FileName.Create(image.FileName).Value;
            var contentType = NonEmptyString.Create(image.ContentType).Value;

            var stream = image.OpenReadStream();

            files.Add(new FileData(stream, fileName, contentType));
        }

        return (
            new CreateProductSkuCommand(
                price.Value,
                quantity,
                size,
                color,
                sku,
                new ProductId(productId),
                files
            ),
            files
        );
    }
}
