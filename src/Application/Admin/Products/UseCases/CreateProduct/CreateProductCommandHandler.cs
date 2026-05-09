using Application.Admin.Products.Errors;
using Domain.Product.Exceptions;

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
        var product = Product.Create(
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

            return Result<Product>.Success(product);
        }
        catch (BrandDoesNotExistsException)
        {
            return Result<Product>.Failure(AdminProductErrors.BrandDoesNotExist(command.BrandId));
        }
        catch (CategoryDoesNotExistsException)
        {
            return Result<Product>.Failure(
                AdminProductErrors.CategoryDoesNotExist(command.CategoryId)
            );
        }
        catch (ProductAlreadyExistsException)
        {
            return Result<Product>.Failure(
                AdminProductErrors.ProductAlreadyExists(
                    command.Title,
                    command.Gender,
                    command.BrandId,
                    command.CategoryId
                )
            );
        }
    }
}
