using Application.Abstractions.Database;
using Application.Admin.Brands.Errors;
using Application.Admin.Brands.Types;
using Domain.Brands.Exceptions;

namespace Application.Admin.Brands.UseCases.CreateBrand;

public sealed record CreateBrandCommand(BrandName Name) : ICommand<AdminBrandResponse>;

internal sealed class CreateBrandCommandHandler(
    IBrandRepository brandRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateBrandCommand, AdminBrandResponse>
{
    public async Task<Result<AdminBrandResponse>> Handle(
        CreateBrandCommand command,
        CancellationToken ct = default
    )
    {
        try
        {
            var newBrand = new Brand(command.Name);
            brandRepository.CreateBrand(newBrand);

            await unitOfWork.SaveChangesAsync(ct);

            return newBrand.ToResponse();
        }
        catch (BrandAlreadyExistsException)
        {
            return AdminBrandErrors.BrandAlreadyExists(command.Name);
        }
    }
}
