using Application.Admin.Products.Errors;

namespace Application.Admin.Products.UseCases.DeleteProduct;

public sealed record ToggleProductIsDeletedCommand(ProductId ProductId) : ICommand;

internal sealed class ToggleProductIsDeletedCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<ToggleProductIsDeletedCommand>
{
    public async Task<Result> Handle(
        ToggleProductIsDeletedCommand command,
        CancellationToken ct = default
    )
    {
        var product = await productRepository.GetProductByIdAsync(command.ProductId, true, ct);
        if (product is null)
        {
            return Result.Failure(AdminProductErrors.ProductNotFound(command.ProductId));
        }

        product.ToggleIsDeleted();
        productRepository.UpdateProduct(product);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
