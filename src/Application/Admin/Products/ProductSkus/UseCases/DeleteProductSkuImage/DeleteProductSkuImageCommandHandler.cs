using System.Text.Json;
using Application.Admin.Products.ProductSkus.Errors;
using Domain.Outbox;
using Domain.Product.ProductSku;
using Domain.Product.ProductSku.ProductSkuImage;

namespace Application.Admin.Products.ProductSkus.UseCases.DeleteProductSkuImage;

public sealed record DeleteProductSkuImageCommand(ProductSkuId Id, ProductSkuImageId ImageId)
    : ICommand;

internal sealed class DeleteProductSkuImageCommandHandler(
    IUnitOfWork unitOfWork,
    IProductSkusRepository productSkusRepository,
    IOutboxRepository outboxRepository
) : ICommandHandler<DeleteProductSkuImageCommand>
{
    public async Task<Result> Handle(
        DeleteProductSkuImageCommand command,
        CancellationToken ct = default
    )
    {
        var productSku = await productSkusRepository.GetProductSkuByIdAsync(command.Id, true, ct);
        if (productSku is null)
        {
            return Result.Failure(AdminProductSkuErrors.ProductSkuNotFound(command.Id));
        }

        var removeImageResult = productSku.RemoveImage(command.ImageId);
        if (removeImageResult.IsFailure)
        {
            return Result.Failure(removeImageResult.Error);
        }

        var outbox = Outbox.Create(
            OutboxType.File,
            NonEmptyString
                .Create(JsonSerializer.Serialize(new List<Guid>() { removeImageResult.Value }))
                .Value
        );

        outboxRepository.CreateOutbox(outbox);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
