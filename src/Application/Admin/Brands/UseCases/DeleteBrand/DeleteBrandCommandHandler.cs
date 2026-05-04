using Application.Admin.Brands.Errors;
using Domain.Product.Brand;

namespace Application.Admin.Brands.UseCases.DeleteBrand;

public sealed record DeleteBrandCommand(BrandId BrandId) : ICommand;

internal sealed class DeleteBrandCommandHandler(
    IBrandRepository brandRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<DeleteBrandCommand>
{
    public async Task<Result> Handle(DeleteBrandCommand command, CancellationToken ct = default)
    {
        var brand = await brandRepository.GetBrandByIdAsync(command.BrandId, true, ct);
        if (brand is null)
        {
            return AdminBrandErrors.BrandNotFound(command.BrandId);
        }

        brandRepository.DeleteBrand(brand);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
