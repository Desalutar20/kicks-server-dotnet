using Application.Abstractions.FileUploader;
using Application.Admin.Products.ProductSkus.Errors;
using Domain.Product.ProductSku.Exceptions;
using File = Application.Abstractions.FileUploader.File;

namespace Application.Admin.Products.ProductSkus.UseCases.CreateProductSku;

public sealed record CreateProductSkuCommand(
    ProductSkuPrice Price,
    PositiveInt Quantity,
    PositiveInt Size,
    ProductSkuColor Color,
    ProductSkuSku Sku,
    ProductId ProductId,
    List<File> Images
) : ICommand<ProductSkuId>;

internal sealed class CreateProductSkuCommandHandler(
    IFileUploader fileUploader,
    IUnitOfWork unitOfWork,
    IProductSkusRepository productSkusRepository
) : ICommandHandler<CreateProductSkuCommand, ProductSkuId>
{
    public async Task<Result<ProductSkuId>> Handle(
        CreateProductSkuCommand command,
        CancellationToken ct = default
    )
    {
        if (await productSkusRepository.ExistsBySkuAsync(command.Sku, ct))
        {
            return AdminProductSkuErrors.ProductSkuAlreadyExists(command.Sku);
        }

        if (
            await productSkusRepository.ExistsByProductSizeColorAsync(
                command.ProductId,
                command.Size,
                command.Color,
                ct
            )
        )
        {
            return AdminProductSkuErrors.ProductSkuDuplicateCombination(
                command.ProductId,
                command.Color,
                command.Size
            );
        }

        var productSku = ProductSku.Create(
            command.Price,
            command.Quantity,
            command.Color,
            command.Sku,
            command.Size,
            command.ProductId
        );

        var tasks = command.Images.Select(file => fileUploader.UploadAsync(file, ct));
        var results = await Task.WhenAll(tasks);

        List<ProductSkuImage> images = [];

        foreach (var uploadResult in results)
        {
            var productSkuImageUrlResult = ProductSkuImageUrl.Create(uploadResult.Uri.ToString());
            if (productSkuImageUrlResult.IsFailure)
            {
                return productSkuImageUrlResult.Error;
            }

            var productSkuImageName = ProductSkuImageName.Create(uploadResult.FileName);
            if (productSkuImageName.IsFailure)
            {
                return productSkuImageName.Error;
            }

            images.Add(
                ProductSkuImage.Create(
                    productSkuImageUrlResult.Value,
                    uploadResult.Id,
                    productSkuImageName.Value,
                    productSku.Id
                )
            );
        }

        var result = productSku.SetImages(images);
        if (result.IsFailure)
        {
            return result.Error;
        }

        productSkusRepository.CreateProductSku(productSku);

        try
        {
            await unitOfWork.SaveChangesAsync(ct);

            return productSku.Id;
        }
        catch (ProductSkuDuplicateCombinationException)
        {
            await Task.WhenAll(results.Select(image => fileUploader.DeleteFileAsync(image.Id)));
            return AdminProductSkuErrors.ProductSkuDuplicateCombination(
                command.ProductId,
                command.Color,
                command.Size
            );
        }
        catch (ProductSkuSkuAlreadyExistsException)
        {
            await Task.WhenAll(results.Select(image => fileUploader.DeleteFileAsync(image.Id)));
            return AdminProductSkuErrors.ProductSkuAlreadyExists(command.Sku);
        }
        catch
        {
            await Task.WhenAll(results.Select(image => fileUploader.DeleteFileAsync(image.Id)));
            throw;
        }
    }
}
