using Application.Abstractions.Database;
using Application.Admin.Products.Errors;
using Application.Admin.Products.ProductSkus.Types;
using Application.Admin.Products.Types;
using Domain.Products.Exceptions;

namespace Application.Admin.Products.UseCases.UpdateProduct;

public sealed record UpdateProductCommand(
    ProductId Id,
    ProductTitle? Title,
    ProductDescription? Description,
    ProductGender? Gender,
    ProductTags? Tags,
    BrandId? BrandId,
    CategoryId? CategoryId
) : ICommand<AdminProductResponse>;

internal sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateProductCommand, AdminProductResponse>
{
    public async Task<Result<AdminProductResponse>> Handle(
        UpdateProductCommand command,
        CancellationToken ct = default
    )
    {
        var product = await productRepository.GetProductByIdAsync(command.Id, true, ct);
        if (product is null)
        {
            return AdminProductErrors.ProductNotFound(command.Id);
        }

        product.Update(
            command.Title ?? product.Title,
            command.Description ?? product.Description,
            command.Gender ?? product.Gender,
            command.Tags ?? product.Tags,
            command.BrandId ?? product.BrandId,
            command.CategoryId ?? product.CategoryId
        );

        productRepository.UpdateProduct(product);

        try
        {
            await unitOfWork.SaveChangesAsync(ct);

            return product.ToDto();
        }
        catch (BrandDoesNotExistsException)
        {
            return AdminProductErrors.BrandDoesNotExist(command.BrandId!);
        }
        catch (CategoryDoesNotExistsException)
        {
            return AdminProductErrors.CategoryDoesNotExist(command.CategoryId!);
        }
        catch (ProductAlreadyExistsException)
        {
            return AdminProductErrors.ProductAlreadyExists(
                command.Title ?? product.Title,
                command.Gender ?? product.Gender,
                command.BrandId ?? product.BrandId,
                command.CategoryId ?? product.CategoryId
            );
        }
    }
}
