using Application.Abstractions.Database;
using Application.Admin.Products.Errors;
using Domain.Products.Exceptions;

namespace Application.Admin.Products.UseCases.CreateProduct;

public sealed record CreateProductCommand(
    ProductTitle Title,
    ProductDescription Description,
    ProductGender Gender,
    ProductTags Tags,
    BrandId BrandId,
    CategoryId CategoryId
) : ICommand<Product>;

internal sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateProductCommand, Product>
{
    public async Task<Result<Product>> Handle(
        CreateProductCommand command,
        CancellationToken ct = default
    )
    {
        var product = new Product(
            command.Title,
            command.Description,
            command.Gender,
            command.Tags,
            command.BrandId,
            command.CategoryId
        );

        productRepository.CreateProduct(product);

        try
        {
            await unitOfWork.SaveChangesAsync(ct);

            return product;
        }
        catch (BrandDoesNotExistsException)
        {
            return AdminProductErrors.BrandDoesNotExist(command.BrandId);
        }
        catch (CategoryDoesNotExistsException)
        {
            return AdminProductErrors.CategoryDoesNotExist(command.CategoryId);
        }
        catch (ProductAlreadyExistsException)
        {
            return AdminProductErrors.ProductAlreadyExists(
                command.Title,
                command.Gender,
                command.BrandId,
                command.CategoryId
            );
        }
    }
}
