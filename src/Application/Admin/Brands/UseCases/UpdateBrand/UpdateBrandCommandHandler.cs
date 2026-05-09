using Application.Admin.Brands.Errors;
using Domain.Product.Brand.Exceptions;

namespace Application.Admin.Brands.UseCases.UpdateBrand;

public sealed record UpdateBrandCommand(BrandId Id, BrandName Name) : ICommand;

internal sealed class UpdateBrandCommandHandler(
    IBrandRepository brandRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateBrandCommand>
{
    public async Task<Result> Handle(UpdateBrandCommand command, CancellationToken ct = default)
    {
        try
        {
            var brand = await brandRepository.GetBrandByIdAsync(command.Id, true, ct);
            if (brand is null)
            {
                return AdminBrandErrors.BrandNotFound(command.Id);
            }

            brand.UpdateName(command.Name);
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (BrandAlreadyExistsException)
        {
            return Result.Failure(AdminBrandErrors.BrandAlreadyExists(command.Name));
        }
    }
}
