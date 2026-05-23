using Application.Abstractions.FileUploader;
using Application.Admin.Products.ProductSkus.Errors;
using Domain.Product.ProductSku.Exceptions;
using File = Application.Abstractions.FileUploader.File;

namespace Application.Admin.Products.ProductSkus.UseCases.UpdateProductSku;

public sealed record UpdateProductSkuCommand(
    ProductSkuId Id,
    PositiveInt? Price,
    PositiveInt? SalePrice,
    PositiveInt? Quantity,
    PositiveInt? Size,
    ProductSkuColor? Color,
    ProductSkuSku? Sku,
    List<File>? Images
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

        var updateResult = productSku.Update(
            price.Value,
            command.Quantity,
            command.Size,
            command.Color,
            command.Sku
        );

        if (updateResult.IsFailure)
        {
            return updateResult.Error;
        }

        FileUploadResult[] uploadedFiles = [];

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
                return addImagesResult.Error;
            }
        }

        try
        {
            await unitOfWork.SaveChangesAsync(ct);

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
            var exists = await productSkusRepository.ExistsBySkuAsync(command.Sku, ct);
            if (exists)
            {
                return AdminProductSkuErrors.ProductSkuAlreadyExists(command.Sku);
            }
        }

        var colorChanged = command.Color is not null && command.Color != productSku.Color;
        var sizeChanged = command.Size is not null && command.Size != productSku.Size;

        if (!colorChanged && !sizeChanged)
            return Result.Success();

        {
            var exists = await productSkusRepository.ExistsByProductSizeColorAsync(
                productSku.ProductId,
                command.Size ?? productSku.Size,
                command.Color ?? productSku.Color,
                ct
            );

            if (exists)
            {
                return AdminProductSkuErrors.ProductSkuDuplicateCombination(
                    productSku.ProductId,
                    command.Color ?? productSku.Color,
                    command.Size ?? productSku.Size
                );
            }
        }

        return Result.Success();
    }

    private async Task<
        Result<(List<ProductSkuImage> images, FileUploadResult[] uploadResults)>
    > UploadImagesAsync(ProductSku productSku, List<File> files, CancellationToken ct)
    {
        var availableSlots = ProductSku.MaxImages - productSku.Images.Images.Count;
        if (availableSlots <= 0)
        {
            return ([], []);
        }

        var uploadTasks = files
            .Take(availableSlots)
            .Select(file => fileUploader.UploadAsync(file, ct));
        var uploadResults = await Task.WhenAll(uploadTasks);

        List<ProductSkuImage> images = [];

        foreach (var uploadResult in uploadResults)
        {
            var urlResult = ProductSkuImageUrl.Create(uploadResult.Uri.ToString());

            if (urlResult.IsFailure)
            {
                return urlResult.Error;
            }

            var nameResult = ProductSkuImageName.Create(uploadResult.FileName);

            if (nameResult.IsFailure)
            {
                return nameResult.Error;
            }

            images.Add(
                ProductSkuImage.Create(
                    urlResult.Value,
                    uploadResult.Id,
                    nameResult.Value,
                    productSku.Id
                )
            );
        }

        return (images, uploadResults);
    }

    private async Task CleanupFilesAsync(FileUploadResult[] uploadedFiles) =>
        await Task.WhenAll(uploadedFiles.Select(x => fileUploader.DeleteFileAsync(x.Id)));
}
