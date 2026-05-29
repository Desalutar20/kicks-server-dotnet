using System.Text.Json;
using Application.Abstractions.Outbox;
using Application.Admin.Products.ProductSkus.Errors;

namespace Application.Admin.Products.ProductSkus.UseCases.DeleteProductSkuImage;

public sealed record DeleteProductSkuImageCommand(ProductSkuId Id, Guid ImageId) : ICommand;

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
            return AdminProductSkuErrors.ProductSkuNotFound(command.Id);
        }

        var removeImageResult = productSku.RemoveImage(command.ImageId);
        if (removeImageResult.IsFailure)
        {
            return removeImageResult.Error;
        }

        var outbox = new Outbox(
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
