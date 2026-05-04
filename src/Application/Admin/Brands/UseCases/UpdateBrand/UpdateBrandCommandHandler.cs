using Application.Admin.Brands.Errors;
using Domain.Product.Brand;

namespace Application.Admin.Brands.UseCases.UpdateBrand;

public sealed record UpdateBrandCommand(BrandId Id, BrandName Name) : ICommand;

internal sealed class UpdateBrandCommandHandler(
    IBrandRepository brandRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateBrandCommand>
{
    public async Task<Result> Handle(UpdateBrandCommand command, CancellationToken ct = default)
    {
        var existingBrand = await brandRepository.GetBrandByNameAsync(command.Name, false, ct);
        if (existingBrand is not null)
        {
            return AdminBrandErrors.BrandAlreadyExists(command.Name);
        }

        var brand = await brandRepository.GetBrandByIdAsync(command.Id, true, ct);
        if (brand is null)
        {
            return AdminBrandErrors.BrandNotFound(command.Id);
        }

        brand.UpdateName(command.Name);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
