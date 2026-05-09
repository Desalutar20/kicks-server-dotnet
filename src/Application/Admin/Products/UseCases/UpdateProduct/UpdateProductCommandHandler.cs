using Application.Admin.Products.Errors;
using Domain.Product.Exceptions;

namespace Application.Admin.Products.UseCases.UpdateProduct;

public sealed record UpdateProductCommand(
    ProductId Id,
    ProductTitle? Title,
    ProductDescription? Description,
    ProductGender? Gender,
    ProductTags? Tags,
    BrandId? BrandId,
    CategoryId? CategoryId
) : ICommand<Product>;

internal sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateProductCommand, Product>
{
    public async Task<Result<Product>> Handle(
        UpdateProductCommand command,
        CancellationToken ct = default
    )
    {
        var product = await productRepository.GetProductByIdAsync(command.Id, true, ct);
        if (product is null)
        {
            return Result<Product>.Failure(AdminProductErrors.ProductNotFound(command.Id));
        }

        product.Update(
            command.Title,
            command.Description,
            command.Gender,
            command.Tags,
            command.BrandId,
            command.CategoryId
        );

        productRepository.UpdateProduct(product);

        try
        {
            await unitOfWork.SaveChangesAsync(ct);

            return Result<Product>.Success(product);
        }
        catch (BrandDoesNotExistsException)
        {
            return Result<Product>.Failure(AdminProductErrors.BrandDoesNotExist(command.BrandId!));
        }
        catch (CategoryDoesNotExistsException)
        {
            return Result<Product>.Failure(
                AdminProductErrors.CategoryDoesNotExist(command.CategoryId!)
            );
        }
        catch (ProductAlreadyExistsException)
        {
            return Result<Product>.Failure(
                AdminProductErrors.ProductAlreadyExists(
                    command.Title ?? product.Title,
                    command.Gender ?? product.Gender,
                    command.BrandId ?? product.BrandId,
                    command.CategoryId ?? product.CategoryId
                )
            );
        }
    }
}
