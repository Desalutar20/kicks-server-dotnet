using Application.Admin.Categories.Errors;
using Domain.Product.Category.Exceptions;

namespace Application.Admin.Categories.UseCases.UpdateCategory;

public sealed record UpdateCategoryCommand(CategoryId Id, CategoryName Name) : ICommand;

internal sealed class UpdateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> Handle(UpdateCategoryCommand command, CancellationToken ct = default)
    {
        try
        {
            var category = await categoryRepository.GetCategoryByIdAsync(command.Id, true, ct);
            if (category is null)
            {
                return AdminCategoryErrors.CategoryNotFound(command.Id);
            }

            category.UpdateName(command.Name);
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (CategoryAlreadyExistsException)
        {
            return Result.Failure(AdminCategoryErrors.CategoryAlreadyExists(command.Name));
        }
    }
}
