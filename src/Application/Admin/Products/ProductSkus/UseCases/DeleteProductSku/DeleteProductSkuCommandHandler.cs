using System.Text.Json;
using Application.Admin.Products.ProductSkus.Errors;
using Domain.Outbox;
using Domain.Product.ProductSku;

namespace Application.Admin.Products.ProductSkus.UseCases.DeleteProductSku;

public sealed record DeleteProductSkuCommand(ProductSkuId Id) : ICommand;

internal sealed class DeleteProductSkuCommandHandler(
    IUnitOfWork unitOfWork,
    IProductSkusRepository productSkusRepository,
    IOutboxRepository outboxRepository
) : ICommandHandler<DeleteProductSkuCommand>
{
    public async Task<Result> Handle(
        DeleteProductSkuCommand command,
        CancellationToken ct = default
    )
    {
        var productSku = await productSkusRepository.GetProductSkuByIdAsync(command.Id, false, ct);
        if (productSku is null)
        {
            return Result.Failure(AdminProductSkuErrors.ProductSkuNotFound(command.Id));
        }

        Outbox? outbox = null;

        if (productSku.ProductSkuImages.Count > 0)
        {
            outbox = Outbox.Create(
                OutboxType.File,
                NonEmptyString
                    .Create(
                        JsonSerializer.Serialize(
                            productSku.ProductSkuImages.Select(image => image.ImageId).ToList()
                        )
                    )
                    .Value
            );
        }

        productSkusRepository.DeleteProductSku(productSku);
        if (outbox is not null)
        {
            outboxRepository.CreateOutbox(outbox);
        }

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
