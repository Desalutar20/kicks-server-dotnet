using Application.Abstractions.Database;
using Application.Abstractions.FileUploader;
using Application.Admin.Products.ProductSkus.Errors;
using Domain.Products.ProductSkus.Exceptions;

namespace Application.Admin.Products.ProductSkus.UseCases.UpdateProductSku;

public sealed record UpdateProductSkuCommand(
    ProductSkuId Id,
    PositiveInt? Price,
    PositiveInt? SalePrice,
    PositiveInt? Quantity,
    PositiveInt? Size,
    ProductSkuColor? Color,
    ProductSkuSku? Sku,
    List<FileData>? Images
) : ICommand<ProductSku>;

internal sealed class UpdateProductSkuCommandHandler(
    IFileUploader fileUploader,
    IUnitOfWork unitOfWork,
    IProductSkusRepository productSkusRepository
) : ICommandHandler<UpdateProductSkuCommand, ProductSku>
{
    public async Task<Result<ProductSku>> Handle(
        UpdateProductSkuCommand command,
        CancellationToken ct = default
    )
    {
        var productSku = await productSkusRepository.GetProductSkuByIdAsync(command.Id, true, ct);
        if (productSku is null)
        {
            return AdminProductSkuErrors.ProductSkuNotFound(command.Id);
        }

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

        var duplicateResult = await ValidateDuplicatesAsync(productSku, command, ct);
        if (duplicateResult.IsFailure)
        {
            return duplicateResult.Error;
        }

        var price = ProductSkuPrice.Create(
            command.Price ?? productSku.Price.Price,
            command.SalePrice ?? productSku.Price.SalePrice
        );
        if (price.IsFailure)
        {
            return price.Error;
        }

        productSku.Update(
            price.Value,
            command.Quantity ?? productSku.Quantity,
            command.Size ?? productSku.Size,
            command.Color ?? productSku.Color,
            command.Sku ?? productSku.Sku
        );

        List<FileUploadResult> uploadedFiles = [];

        try
        {
            await unitOfWork.SaveChangesAsync(ct);

            if (command.Images is not null && command.Images.Count > 0)
            {
                var imagesResult = await UploadImagesAsync(productSku, command.Images, ct);
                if (imagesResult.IsFailure)
                {
                    return imagesResult.Error;
                }

                uploadedFiles = imagesResult.Value.uploadResults;

                var addImagesResult = productSku.AddImages(imagesResult.Value.images);
                if (addImagesResult.IsFailure)
                {
                    await CleanupFilesAsync(uploadedFiles);
                    return addImagesResult.Error;
                }
            }

            await transaction.CommitAsync(ct);

            return productSku;
        }
        catch (ProductSkuDuplicateCombinationException)
        {
            await CleanupFilesAsync(uploadedFiles);
            return AdminProductSkuErrors.ProductSkuDuplicateCombination(
                productSku.ProductId,
                command.Color ?? productSku.Color,
                command.Size ?? productSku.Size
            );
        }
        catch (ProductSkuSkuAlreadyExistsException)
        {
            await CleanupFilesAsync(uploadedFiles);
            return AdminProductSkuErrors.ProductSkuAlreadyExists(command.Sku ?? productSku.Sku);
        }
        catch
        {
            await CleanupFilesAsync(uploadedFiles);
            throw;
        }
    }

    private async Task<Result> ValidateDuplicatesAsync(
        ProductSku productSku,
        UpdateProductSkuCommand command,
        CancellationToken ct
    )
    {
        if (command.Sku is not null && command.Sku != productSku.Sku)
        {
            var existsBySku = await productSkusRepository.ExistsBySkuAsync(command.Sku, ct);
            if (existsBySku)
            {
                return AdminProductSkuErrors.ProductSkuAlreadyExists(command.Sku);
            }
        }

        var colorChanged = command.Color is not null && command.Color != productSku.Color;
        var sizeChanged = command.Size is not null && command.Size != productSku.Size;

        if (!colorChanged && !sizeChanged)
            return Result.Success();

        var existsBySizeOrColor = await productSkusRepository.ExistsByProductSizeColorAsync(
            productSku.ProductId,
            command.Size ?? productSku.Size,
            command.Color ?? productSku.Color,
            ct
        );

        if (existsBySizeOrColor)
        {
            return AdminProductSkuErrors.ProductSkuDuplicateCombination(
                productSku.ProductId,
                command.Color ?? productSku.Color,
                command.Size ?? productSku.Size
            );
        }

        return Result.Success();
    }

    private async Task<
        Result<(List<ProductSkuImage> images, List<FileUploadResult> uploadResults)>
    > UploadImagesAsync(ProductSku productSku, List<FileData> files, CancellationToken ct)
    {
        if (productSku.RemainingImageSlots <= 0)
        {
            return ([], []);
        }

        var uploadResults = await fileUploader.UploadFilesAsync(
            files.Take(productSku.RemainingImageSlots),
            ct
        );
        if (uploadResults.IsFailure)
            return uploadResults.Error;

        var images = uploadResults.Value.Select(result =>
            ProductSkuImage.Create(result.Id, result.Uri, result.FileName)
        );

        return (images.ToList(), uploadResults.Value.ToList());
    }

    private async Task CleanupFilesAsync(IEnumerable<FileUploadResult> uploadResults) =>
        await fileUploader.DeleteFilesAsync(uploadResults.Select(image => image.Id));
}
