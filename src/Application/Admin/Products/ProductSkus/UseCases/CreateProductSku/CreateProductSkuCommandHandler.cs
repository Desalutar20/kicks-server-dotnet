using Application.Abstractions.Database;
using Application.Abstractions.FileUploader;
using Application.Abstractions.Messaging;
using Application.Admin.Products.ProductSkus.Errors;
using Application.ProductSkus;
using Domain.Products.ProductSkus.Exceptions;
using Domain.Shared.ValueObjects;

namespace Application.Admin.Products.ProductSkus.UseCases.CreateProductSku;

public sealed record CreateProductSkuCommand(
    ProductSkuPrice Price,
    PositiveInt Quantity,
    PositiveInt Size,
    ProductSkuColor Color,
    ProductSkuSku Sku,
    ProductId ProductId,
    List<FileData> Images
) : ICommand<ProductSkuId>;

internal sealed class CreateProductSkuCommandHandler(
    IFileUploader fileUploader,
    IUnitOfWork unitOfWork,
    IProductSkusRepository productSkusRepository,
    IProductSkusReadRepository productSkusReadRepository,
    IMessageQueue<IEnumerable<FileUploadResult>> messageQueue
) : ICommandHandler<CreateProductSkuCommand, ProductSkuId>
{
    public async Task<Result<ProductSkuId>> Handle(
        CreateProductSkuCommand command,
        CancellationToken ct = default
    )
    {
        var duplicateResult = await ValidateDuplicateAsync(command, ct);
        if (duplicateResult.IsFailure)
        {
            return duplicateResult.Error;
        }

        var uploadResults = new List<FileUploadResult>();

        var uploadResult = await UploadImagesAsync(command.Images, ct);
        if (uploadResult.IsFailure)
        {
            return uploadResult.Error;
        }

        var (images, results) = uploadResult.Value;
        uploadResults = results;

        var result = ProductSku.Create(
            command.Price,
            command.Quantity,
            command.Color,
            command.Sku,
            command.Size,
            images,
            command.ProductId
        );

        if (result.IsFailure)
        {
            await CleanupFilesAsync(uploadResults);

            return result.Error;
        }

        productSkusRepository.CreateProductSku(result.Value);

        try
        {
            await unitOfWork.SaveChangesAsync(ct);

            return result.Value.Id;
        }
        catch (ProductDoesNotExistsException)
        {
            await CleanupFilesAsync(uploadResults);

            return AdminProductSkuErrors.ProductDoesNotExist(command.ProductId);
        }
        catch (ProductSkuDuplicateCombinationException)
        {
            await CleanupFilesAsync(uploadResults);

            return AdminProductSkuErrors.ProductSkuDuplicateCombination(
                command.ProductId,
                command.Color,
                command.Size
            );
        }
        catch (ProductSkuSkuAlreadyExistsException)
        {
            await CleanupFilesAsync(uploadResults);

            return AdminProductSkuErrors.ProductSkuAlreadyExists(command.Sku);
        }
        catch
        {
            await CleanupFilesAsync(uploadResults);
            throw;
        }
    }

    private async Task<Result> ValidateDuplicateAsync(
        CreateProductSkuCommand command,
        CancellationToken ct
    )
    {
        if (await productSkusReadRepository.ExistsBySkuAsync(command.Sku, ct))
        {
            return AdminProductSkuErrors.ProductSkuAlreadyExists(command.Sku);
        }

        if (
            await productSkusReadRepository.ExistsByProductSizeColorAsync(
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

        return Result.Success();
    }

    private async Task<
        Result<(List<ProductSkuImage> images, List<FileUploadResult> uploadResults)>
    > UploadImagesAsync(List<FileData> files, CancellationToken ct)
    {
        var uploadResults = await fileUploader.UploadFilesAsync(files, ct);
        if (uploadResults.IsFailure)
        {
            return uploadResults.Error;
        }

        var images = uploadResults
            .Value.Select(result => ProductSkuImage.Create(result.Id, result.Uri, result.FileName))
            .ToList();

        return (images, uploadResults.Value.ToList());
    }

    private async Task CleanupFilesAsync(List<FileUploadResult> uploadResults)
    {
        if (uploadResults.Count > 0)
            await messageQueue.WriteAsync(uploadResults);
    }
}
