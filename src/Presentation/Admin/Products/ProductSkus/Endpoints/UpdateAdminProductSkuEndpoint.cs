using Application.Abstractions.FileUploader;
using Application.Admin.Products.ProductSkus.Constants;
using Application.Admin.Products.ProductSkus.Types;
using Application.Admin.Products.ProductSkus.UseCases.UpdateProductSku;
using Application.Auth.Types;
using Domain.Products.ProductSkus;
using Domain.Shared.FileContent;
using Domain.Shared.ValueObjects;
using Presentation.Shared.Extensions;

namespace Presentation.Admin.Products.ProductSkus.Endpoints;

public sealed record UpdateProductSkuRequest
{
    [FromForm(Name = "price")]
    public decimal? Price { get; set; }

    [FromForm(Name = "salePrice")]
    public decimal? SalePrice { get; set; }

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
        RuleFor(x => x.Price)
            .GreaterThan(0)
            .ValidateNullableValueObject(x => Money.FromDollars(x!.Value))
            .When(x => x.Price is not null);
        RuleFor(x => x.SalePrice)
            .GreaterThan(0)
            .ValidateNullableValueObject(x => Money.FromDollars(x!.Value))
            .When(x => x.SalePrice is not null);

        RuleFor(x => x)
            .Must(x => x.SalePrice == null || x.Price is null || x.SalePrice < x.Price)
            .WithMessage("Sale price must be less than price.")
            .WithName("salePrice");

        RuleFor(x => x.Quantity)
            .ValidateNullableValueObject(x => PositiveInt.Create(x!.Value, label: "Quantity"));
        RuleFor(x => x.Size)
            .ValidateNullableValueObject(x => PositiveInt.Create(x!.Value, label: "Size"));

        RuleFor(x => x.Color).ValidateNullableValueObject(ProductSkuColor.Create);
        RuleFor(x => x.Sku).ValidateNullableValueObject(ProductSkuSku.Create);

        RuleFor(x => x.Images)
            .Must(x => x is null || x.Count <= ProductSku.MaxImages)
            .WithMessage($"Max {ProductSku.MaxImages} images allowed.");

        RuleFor(x => x.Images)
            .Must(x =>
                x is null
                || x.All(file =>
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
                    ICommandHandler<
                        UpdateProductSkuCommand,
                        AdminProductSkuResponse
                    > commandHandler,
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
                        return commandResult.Error.ToApiError(logger);
                    }

                    var (command, files) = commandResult.Value;

                    Result<AdminProductSkuResponse> result;

                    try
                    {
                        result = await commandHandler.Handle(command, ct);
                    }
                    finally
                    {
                        if (files is not null)
                            await Task.WhenAll(
                                files.Select(async f => await f.Content.DisposeAsync())
                            );
                    }

                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
                        : Results.Ok(new ApiResponse<AdminProductSkuResponse>(result.Value));
                }
            )
            .AddEndpointFilter<AuthenticateFilter>()
            .AddEndpointFilter(new AuthorizeFilter(Role.Admin))
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<AdminProductSkuResponse>>()
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

    private static Result<(UpdateProductSkuCommand command, List<FileData>? streams)> ToCommand(
        this UpdateProductSkuRequest request,
        Guid productSkuId
    )
    {
        var price = request.Price is not null ? Money.FromDollars(request.Price.Value).Value : null;
        var salePrice = request.SalePrice is not null
            ? Money.FromDollars(request.SalePrice.Value).Value
            : null;
        var quantity = request.Quantity is not null
            ? PositiveInt.Create(request.Quantity.Value).Value
            : null;
        var size = request.Size is not null ? PositiveInt.Create(request.Size.Value).Value : null;
        var color = request.Color is not null ? ProductSkuColor.Create(request.Color).Value : null;
        var sku = request.Sku is not null ? ProductSkuSku.Create(request.Sku).Value : null;

        List<FileData>? files = null;

        if (request.Images is not null && request.Images.Count > 0)
        {
            files = [];

            foreach (var image in request.Images)
            {
                var fileName = FileName.Create(image.FileName).Value;
                var contentType = NonEmptyString.Create(image.ContentType).Value;

                var stream = image.OpenReadStream();

                files.Add(new FileData(stream, fileName, contentType));
            }
        }

        return (
            new UpdateProductSkuCommand(
                new ProductSkuId(productSkuId),
                price,
                salePrice,
                quantity,
                size,
                color,
                sku,
                files
            ),
            files
        );
    }
}
