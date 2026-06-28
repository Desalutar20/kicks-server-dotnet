using Application.Abstractions.Database;
using Application.Admin.Products.Errors;
using Application.Admin.Products.ProductSkus.Types;
using Application.Admin.Products.Types;
using Domain.Products.Exceptions;

namespace Application.Admin.Products.UseCases.CreateProduct;

public sealed record CreateProductCommand(
    ProductTitle Title,
    ProductDescription Description,
    ProductGender Gender,
    ProductTags Tags,
    BrandId BrandId,
    CategoryId CategoryId
) : ICommand<AdminProductResponse>;

internal sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateProductCommand, AdminProductResponse>
{
    public async Task<Result<AdminProductResponse>> Handle(
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

            return product.ToAdminResponse();
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
