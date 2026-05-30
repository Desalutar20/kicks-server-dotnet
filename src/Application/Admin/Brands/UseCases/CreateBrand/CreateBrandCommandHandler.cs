using Application.Abstractions.Database;
using Application.Admin.Brands.Errors;
using Domain.Brands.Exceptions;

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
            var newBrand = new Brand(command.Name);
            brandRepository.CreateBrand(newBrand);

            await unitOfWork.SaveChangesAsync(ct);

            return newBrand;
        }
        catch (BrandAlreadyExistsException)
        {
            return AdminBrandErrors.BrandAlreadyExists(command.Name);
        }
    }
}
