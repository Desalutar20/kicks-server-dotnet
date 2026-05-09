using Application.Admin.Brands.Errors;
using Domain.Product.Brand.Exceptions;

namespace Application.Admin.Brands.UseCases.CreateBrand;

public sealed record CreateBrandCommand(BrandName Name) : ICommand<Brand>;

internal sealed class CreateBrandCommandHandler(
    IBrandRepository brandRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateBrandCommand, Brand>
{
    public async Task<Result<Brand>> Handle(
        CreateBrandCommand command,
        CancellationToken ct = default
    )
    {
        try
        {
            var newBrand = Brand.Create(command.Name);
            brandRepository.CreateBrand(newBrand);

            await unitOfWork.SaveChangesAsync(ct);

            return Result<Brand>.Success(newBrand);
        }
        catch (BrandAlreadyExistsException)
        {
            return Result<Brand>.Failure(AdminBrandErrors.BrandAlreadyExists(command.Name));
        }
    }
}
