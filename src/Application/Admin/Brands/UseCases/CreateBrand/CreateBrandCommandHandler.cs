using Application.Admin.Brands.Errors;
using Domain.Product.Brand;

namespace Application.Admin.Brands.UseCases.CreateBrand;

public sealed record CreateBrandCommand(BrandName Name) : ICommand;

internal sealed class CreateBrandCommandHandler(
    IBrandRepository brandRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateBrandCommand>
{
    public async Task<Result> Handle(CreateBrandCommand command, CancellationToken ct = default)
    {
        var brand = await brandRepository.GetBrandByNameAsync(command.Name, false, ct);
        if (brand is not null)
        {
            return AdminBrandErrors.BrandAlreadyExists(command.Name);
        }

        var newBrand = Brand.Create(command.Name);
        brandRepository.CreateBrand(newBrand);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
