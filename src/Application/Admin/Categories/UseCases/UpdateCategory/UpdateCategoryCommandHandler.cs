using Application.Admin.Categories.Errors;
using Domain.Product.Category;

namespace Application.Admin.Categories.UseCases.UpdateCategory;

public sealed record UpdateCategoryCommand(CategoryId Id, CategoryName Name) : ICommand;

internal sealed class UpdateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> Handle(UpdateCategoryCommand command, CancellationToken ct = default)
    {
        var existingCategory = await categoryRepository.GetCategoryByNameAsync(
            command.Name,
            false,
            ct
        );
        if (existingCategory is not null)
        {
            return Result.Failure(AdminCategoriesErrors.CategoryAlreadyExists(command.Name));
        }

        var category = await categoryRepository.GetCategoryByIdAsync(command.Id, true, ct);
        if (category is null)
        {
            return AdminCategoriesErrors.CategoryNotFound(command.Id);
        }

        category.UpdateName(command.Name);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
